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
builder.Services.AddServerSideBlazor()
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
    // The ingress controller is a trusted proxy inside the cluster.
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

app.UseStaticFiles();

app.UseRouting();

// Add the authentication/authorization middleware explicitly so it runs after
// UseForwardedHeaders and UsePathBase. If left implicit, WebApplication inserts
// it ahead of that middleware, and the OIDC redirect_uri ends up without the
// external https scheme and the /planner path base.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
