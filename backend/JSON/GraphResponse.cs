using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace planner_exandimport_wasm.backend.JSON;

// Describes the target of an outbound Graph request: the base URL to append to
// and the bearer token to authenticate with. Replaces the Fermyon Spin
// HttpRequest that was previously used as the outbound request carrier.
public class GraphRequest
{
    public string Url { get; set; } = "";
    public string Token { get; set; } = "";
}

// provides generic convenience methods to make get, post and patch requests
public class GraphResponse<Type>
{
    // a single shared HttpClient for all outbound Graph calls; the per-request
    // authorization header is set on each HttpRequestMessage instead of the client,
    // so sharing the instance across tokens is safe
    private static readonly HttpClient _httpClient = new HttpClient();

    private static Type? FromJson(string json)
    {
        return JsonSerializer.Deserialize<Type>(json);
    }

    public static Type? Get(string url, GraphRequest outboundReq)
    {
        return Send(HttpMethod.Get, outboundReq.Url + url, outboundReq.Token);
    }

    public static Type? Post(string url, GraphRequest outboundReq, Type data)
    {
        string content = JsonSerializer.Serialize(data, Handler.DefaultOptions);
        return Send(HttpMethod.Post, outboundReq.Url + url, outboundReq.Token, content);
    }

    public static Type? Patch(string url, GraphRequest outboundReq, Type data, string ifMatch)
    {
        string content = JsonSerializer.Serialize(data, Handler.DefaultOptions);
        return Send(HttpMethod.Patch, outboundReq.Url + $"/{url}", outboundReq.Token, content, ifMatch);
    }

    private static Type? Send(HttpMethod method, string url, string token, string? content = null, string? ifMatch = null)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.TryAddWithoutValidation("Authorization", token);
        if (ifMatch != null)
            request.Headers.TryAddWithoutValidation("If-Match", ifMatch);

        if (content != null)
        {
            Handler._logger.LogDebug($"content: {content}");
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
        }

        using var response = _httpClient.Send(request);
        var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(body, null, response.StatusCode);
        if (string.IsNullOrEmpty(body))
            return default(Type);
        return FromJson(body);
    }
}
