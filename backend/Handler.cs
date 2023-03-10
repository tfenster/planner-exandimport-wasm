using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using Fermyon.Spin.Sdk;
using Microsoft.Extensions.Logging;
using planner_exandimport_wasm.shared.JSON;

namespace planner_exandimport_wasm;

public static class Handler
{
    public static ILogger _logger;

    // https://github.com/christophwille/SpinHello/blob/abef0de9810abe2894f1906e5dd740e672f19b34/src/Handler/Handler.cs#L98
    private delegate Task<HttpResponse> RequestHandlerDelegate(HttpRequest request);
    private static Dictionary<string, RequestHandlerDelegate> _routes = new Dictionary<string, RequestHandlerDelegate>()
    {
        { "/groups", GroupsHandler },
        { "/plans", PlansHandler },
        { "/planDetails", PlanDetailsHandler },
        { "/duplicatePlan", DuplicatePlanHandler },
        { "/duplicateBucket", DuplicateBucketHandler },
        { "/user", GetUserByIdHandler },
        { "/echo", EchoHandler }
    };

    public static JsonSerializerOptions DefaultOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    static Handler()
    {
        _logger = new SpinLogger();
    }

    [HttpHandler]
    public static HttpResponse HandleHttpRequest(HttpRequest request)
    {
        if (request.Url == Warmup.DefaultWarmupUrl)
            return new HttpResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "text/plain" },
                },
                BodyAsString = "warmup",
            };

        _logger.LogInformation($"Got request: {JsonSerializer.Serialize(request.Url)}");

        try
        {
            var requestPath = request.Headers["spin-path-info"];
            var routeFound = _routes.TryGetValue(requestPath, out var handler);

            if (routeFound && null != handler)
                return handler(request).Result;
            else
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "text/plain" },
                    },
                    BodyAsString = "Requested route not found",
                };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during request handling:");
            _logger.LogError($"{ex.Message}");
            _logger.LogError(ex.StackTrace);
            return BadRequestException(ex);
        }
    }

#pragma warning disable 1998
    private async static Task<HttpResponse> EchoHandler(HttpRequest request)
    {
        var headers = request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        if (headers.ContainsKey("content-type"))
        {
            headers["x-content-type"] = headers["content-type"];
            headers.Remove("content-type");
        }
        headers.Add("content-type", "application/json; charset=utf-8");
        headers.Add("x-parameters", string.Join(",", request.Parameters));

        var sb = new StringBuilder();
        var parsedParameters = request.ParsedParameters();
        foreach (var name in parsedParameters.AllKeys)
            sb.Append($"{name} - {parsedParameters[name]}, ");
        headers.Add("x-parsed-parameters", sb.ToString());

        return OkObject(request.Body.AsString());
    }
#pragma warning restore 1998

    private async static Task<HttpResponse> GroupsHandler(HttpRequest request)
    {
        var planner = new Planner(request.Headers["authorization"]);
        var groups = await planner.GetGroups(request.ParsedParameters().Get("groupSearch"));
        if (groups == null)
            return NotFound();
        return OkObject(JsonSerializer.Serialize(groups));
    }

    private async static Task<HttpResponse> PlansHandler(HttpRequest request)
    {
        var planner = new Planner(request.Headers["authorization"]);
        var plans = await planner.GetPlans(request.ParsedParameters().Get("groupId"));
        if (plans == null)
            return NotFound();
        return OkObject(JsonSerializer.Serialize(plans));
    }

    private async static Task<HttpResponse> PlanDetailsHandler(HttpRequest request)
    {
        var planner = new Planner(request.Headers["authorization"]);
        var plan = await planner.GetPlanDetails(request.ParsedParameters().Get("groupId"), request.ParsedParameters().Get("planId"));
        if (plan == null)
            return NotFound();
        return OkObject(JsonSerializer.Serialize(plan, DefaultOptions));
    }

    private async static Task<HttpResponse> DuplicatePlanHandler(HttpRequest request)
    {
        var planner = new Planner(request.Headers["authorization"]);
        DuplicationAdjustments? duplicationAdjustments = null;
        var body = request.Body.AsString();
        if (!string.IsNullOrEmpty(body))
            duplicationAdjustments = JsonSerializer.Deserialize<DuplicationAdjustments>(body);
        var plan = await planner.DuplicatePlan(request.ParsedParameters().Get("sourceGroupId"), request.ParsedParameters().Get("sourcePlanId"),
            request.ParsedParameters().Get("targetGroupId"), request.ParsedParameters().Get("targetPlanId"),
            duplicationAdjustments);
        if (plan == null)
            return BadRequestString("Failed to duplicate plan");
        return OkObject(JsonSerializer.Serialize(plan, DefaultOptions));
    }

    private async static Task<HttpResponse> DuplicateBucketHandler(HttpRequest request)
    {
        var planner = new Planner(request.Headers["authorization"]);
        BucketWithDuplicationAdjustments? bucketWithDuplicationAdjustments = null;
        var body = request.Body.AsString();
        if (!string.IsNullOrEmpty(body))
            bucketWithDuplicationAdjustments = JsonSerializer.Deserialize<BucketWithDuplicationAdjustments>(body);
        if (bucketWithDuplicationAdjustments?.Bucket == null)
            return BadRequestString("Couldn't parse the expected bucket and duplication adjustments in the body");
        var bucketId = await planner.DuplicateBucket(request.ParsedParameters().Get("targetPlanId"), bucketWithDuplicationAdjustments);
        if (bucketId == null)
            return BadRequestString("Failed to duplicate bucket");
        return OkObject(bucketId);
    }

    private async static Task<HttpResponse> GetUserByIdHandler(HttpRequest request)
    {
        var planner = new Planner(request.Headers["authorization"]);
        var user = await planner.GetGraphUser(request.ParsedParameters().Get("userIdOrEmail"));
        if (user == null)
            return BadRequestString($"Failed to identify user by id or email {request.ParsedParameters().Get("userIdOrEmail")}");
        return OkObject(JsonSerializer.Serialize(user, DefaultOptions));
    }

    private static HttpResponse NotFound()
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.NotFound
        };
    }

    private static HttpResponse OkObject(string s)
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/json" },
            },
            BodyAsString = s,
        };
    }

    private static HttpResponse BadRequestString(string s)
    {
        return new HttpResponse
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
            },
            BodyAsString = s,
        };
    }

    private static HttpResponse BadRequestException(Exception ex)
    {
        var statusCode = HttpStatusCode.BadRequest;
        if (ex is HttpRequestException)
        {
            var hre = ex as HttpRequestException;
            if (hre != null && hre.StatusCode != null)
                statusCode = (HttpStatusCode)hre.StatusCode;
        }
        return new HttpResponse
        {
            StatusCode = statusCode,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
            },
            BodyAsString = ex.ToString(),
        };
    }
}

static class QueryStringParser
{
    public static NameValueCollection ParsedParameters(this HttpRequest httpRequest)
    {
        var indexOfQuestionMark = httpRequest.Url.IndexOf("?");
        var url = httpRequest.Url;
        if (indexOfQuestionMark > 0)
        {
            url = url.Substring(indexOfQuestionMark + 1);
            return HttpUtility.ParseQueryString(url);
        }
        else
            return new NameValueCollection();
    }
}
