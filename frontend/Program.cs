using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpOverrides;
using planner_exandimport_wasm.shared.JSON;
using AntDesign;
using planner_exandimport_wasm.frontend.Data;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web.UI;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddAntDesign();
builder.Services.AddScoped<BackendService>();

// When running behind the AKS ingress (which terminates TLS), honor the
// X-Forwarded-* headers so the app sees the original https scheme and host.
// This is required for the OIDC redirect URIs to be generated as https and
// match what is registered in Azure AD.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Clearing the known networks/proxies makes X-Forwarded-* trusted from any
    // caller. This is safe here because the pod is only reachable through the
    // nginx ingress (ClusterIP Service, no NodePort/hostPort), so an untrusted
    // client can't reach it directly to spoof the scheme/remote IP. If the app
    // is ever exposed without the ingress in front, restrict these to the
    // ingress controller's network/CIDR instead.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Allow hosting under a sub-path behind the ingress (e.g. PathBase=/planner).
var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCookiePolicy(new CookiePolicyOptions()
{
    MinimumSameSitePolicy = SameSiteMode.Unspecified
});

app.UseHttpsRedirection();

// Serve physical wwwroot files (incl. _framework/blazor.web.js) up front, before
// routing/authorization, so the Blazor boot script isn't blocked by the global
// RequireAuthenticatedUser fallback policy. A Blazor Server app needs this in
// addition to MapStaticAssets. See the Blazor "static files" docs.
app.UseStaticFiles();

app.UseRouting();

// Add the authentication/authorization middleware explicitly so it runs after
// UseForwardedHeaders and UsePathBase. If left implicit, WebApplication inserts
// it ahead of that middleware, and the OIDC redirect_uri ends up without the
// external https scheme and the /planner path base.
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// MapStaticAssets serves the app's static web assets, including the Blazor
// framework script (blazor.web.js) that boots the interactive server circuit.
// AllowAnonymous so the global RequireAuthenticatedUser fallback policy doesn't
// block the boot script and other assets (pages stay protected by the policy
// and AuthorizeRouteView).
app.MapStaticAssets().AllowAnonymous();
app.MapControllers();
app.MapRazorPages();
app.MapRazorComponents<planner_exandimport_wasm.frontend.App>()
    .AddInteractiveServerRenderMode();

app.Run();
