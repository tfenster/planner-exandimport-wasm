using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Authorization;
using planner_exandimport_wasm.shared.JSON;
using Microsoft.Identity.Web;

namespace planner_exandimport_wasm.frontend.Data;

public class BackendService
{
    private readonly IConfiguration _configuration;

    public BackendService(IConfiguration configuration)
    {
        _configuration = configuration;
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

    private async Task SetupClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetValue<string>("GraphToken"));
        client.BaseAddress = new Uri(_configuration.GetValue<string>("BackendBaseUrl")!);
    }
}