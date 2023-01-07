using System.Text.Json;
using Fermyon.Spin.Sdk;
using HttpMethod = Fermyon.Spin.Sdk.HttpMethod;
using Microsoft.Extensions.Logging;

namespace planner_exandimport_wasm.backend.JSON;

// provides generic convenience methods to make get, post and patch requests
public class GraphResponse<Type>
{
    private static Type? FromJson(string json)
    {
        return JsonSerializer.Deserialize<Type>(json);
    }

    public static Type? Get(string url, HttpRequest outboundReq)
    {
        outboundReq.Url += url;
        outboundReq.Method = HttpMethod.Get;
        return Send(outboundReq);
    }

    public static Type? Post(string url, HttpRequest outboundReq, Type data)
    {
        string content = JsonSerializer.Serialize(data, Handler.DefaultOptions);
        outboundReq.Url += url;
        outboundReq.Method = HttpMethod.Post;
        return Send(outboundReq, content);
    }

    public static Type? Patch(string url, HttpRequest outboundReq, Type data, string ifMatch)
    {
        string content = JsonSerializer.Serialize(data, Handler.DefaultOptions);
        outboundReq.Url += $"/{url}";
        outboundReq.Method = HttpMethod.Patch;
        outboundReq.Headers = outboundReq.Headers.Append(new KeyValuePair<string, string>("If-Match", ifMatch)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        return Send(outboundReq, content);
    }

    private static Type? Send(HttpRequest outboundReq, string? content = null)
    {
        if (content != null)
        {
            Handler._logger.LogDebug($"content: {content}");
            outboundReq.Body = Optional.From(Buffer.FromString(content));
            outboundReq.Headers = outboundReq.Headers.Append(new KeyValuePair<string, string>("Content-type", "application/json")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        Handler._logger.LogDebug($"outbound req: {JsonSerializer.Serialize(outboundReq)}");
        var response = HttpOutbound.Send(outboundReq);
        Handler._logger.LogDebug($"outbound response: {JsonSerializer.Serialize(response)}");
        response.EnsureSuccessStatusCode();
        if (string.IsNullOrEmpty(response.BodyAsString))
            return default(Type);
        return FromJson(response.BodyAsString);
    }
}

public static class SuccessCheck
{
    // roughly aligned with https://github.com/microsoft/referencesource/blob/master/System/net/System/Net/Http/HttpResponseMessage.cs
    public static bool IsSuccessStatusCode(this HttpResponse response)
    {
        return ((int)response.StatusCode >= 200) && ((int)response.StatusCode <= 299);
    }

    public static HttpResponse EnsureSuccessStatusCode(this HttpResponse response)
    {
        if (!response.IsSuccessStatusCode())
        {
            throw new HttpRequestException($"{response.BodyAsString}", null, response.StatusCode);
        }
        return response;
    }
}