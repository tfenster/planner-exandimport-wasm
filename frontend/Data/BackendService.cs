using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Authorization;
using planner_exandimport_wasm.shared.JSON;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace planner_exandimport_wasm.frontend.Data;

public class BackendService
{
    private readonly IConfiguration _configuration;
    private readonly ITokenAcquisition _tokenAcquisition;

    public BackendService(IConfiguration configuration, ITokenAcquisition tokenAcquisition)
    {
        _configuration = configuration;
        _tokenAcquisition = tokenAcquisition;
    }

    public async Task<Group[]?> GetGroups(string? searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString) || searchString.Length < 3)
            return Array.Empty<Group>();

        using HttpClient client = new();
        await SetupClient(client);

        await using Stream stream = await client.GetStreamAsync($"/groups?groupSearch={searchString}");
        var groups = await JsonSerializer.DeserializeAsync<Group[]>(stream);
        return groups;
    }

    public async Task<Plan[]?> GetPlans(string? groupId)
    {
        if (string.IsNullOrWhiteSpace(groupId))
            return Array.Empty<Plan>();

        using HttpClient client = new();
        await SetupClient(client);

        await using Stream stream = await client.GetStreamAsync($"/plans?groupId={groupId}");
        var plans = await JsonSerializer.DeserializeAsync<Plan[]>(stream);
        return plans;
    }

    public async Task<GraphUser?> GetUser(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        using HttpClient client = new();
        await SetupClient(client);

        await using Stream stream = await client.GetStreamAsync($"/user?userIdOrEmail={email}");
        var user = await JsonSerializer.DeserializeAsync<GraphUser>(stream);
        return user;
    }

    public async Task<Plan?> DuplicatePlan(string? sourceGroupId, string? sourcePlanId, string? targetGroupId, string? targetPlanId, DuplicationAdjustments duplicationAdjustments)
    {
        if (string.IsNullOrWhiteSpace(sourcePlanId) || string.IsNullOrWhiteSpace(targetPlanId) || string.IsNullOrWhiteSpace(sourceGroupId) || string.IsNullOrWhiteSpace(targetGroupId))
            return null;

        using HttpClient client = new();
        await SetupClient(client);

        var json = JsonSerializer.Serialize(duplicationAdjustments);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var httpResponseMessage = await client.PostAsync($"/duplicatePlan?sourceGroupId={sourceGroupId}&sourcePlanId={sourcePlanId}&targetGroupId={targetGroupId}&targetPlanId={targetPlanId}", content);
        httpResponseMessage.EnsureSuccessStatusCode();
        await using Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync();
        var plan = await JsonSerializer.DeserializeAsync<Plan>(stream);
        return plan;
    }

    private async Task SetupClient(HttpClient client)
    {
        var scopes = _configuration.GetSection("AzureAd:Scopes").Get<List<string>>();
        var backendUrl = _configuration.GetValue<string>("BackendBaseUrl");
        if (scopes == null || scopes.Count == 0)
            throw new InvalidOperationException("No scopes defined in AzureAd:Scopes, configuration is incomplete.");
        if (string.IsNullOrWhiteSpace(backendUrl))
            throw new InvalidOperationException("No BackendBaseUrl defined in configuration, configuration is incomplete.");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await _tokenAcquisition.GetAccessTokenForUserAsync(scopes.ToArray())));
        client.BaseAddress = new Uri(backendUrl);
    }
}