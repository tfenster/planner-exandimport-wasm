using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using planner_exandimport_wasm;
using planner_exandimport_wasm.shared.JSON;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// wire the shared logger used by Planner and the Graph helper into the host logger
Handler._logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("planner_exandimport_wasm");

app.MapGet("/groups", (HttpRequest request) => Handle(async () =>
{
    var planner = new Planner(Authorization(request));
    var groups = await planner.GetGroups(request.Query["groupSearch"]);
    if (groups == null)
        return NotFound();
    return OkObject(JsonSerializer.Serialize(groups));
}));

app.MapGet("/plans", (HttpRequest request) => Handle(async () =>
{
    var planner = new Planner(Authorization(request));
    var plans = await planner.GetPlans(request.Query["groupId"]);
    if (plans == null)
        return NotFound();
    return OkObject(JsonSerializer.Serialize(plans));
}));

app.MapGet("/planDetails", (HttpRequest request) => Handle(async () =>
{
    var planner = new Planner(Authorization(request));
    var plan = await planner.GetPlanDetails(request.Query["groupId"], request.Query["planId"]);
    if (plan == null)
        return NotFound();
    return OkObject(JsonSerializer.Serialize(plan, Handler.DefaultOptions));
}));

app.MapPost("/duplicatePlan", (HttpRequest request) => Handle(async () =>
{
    var planner = new Planner(Authorization(request));
    DuplicationAdjustments? duplicationAdjustments = null;
    var body = await ReadBody(request);
    if (!string.IsNullOrEmpty(body))
        duplicationAdjustments = JsonSerializer.Deserialize<DuplicationAdjustments>(body);
    var plan = await planner.DuplicatePlan(request.Query["sourceGroupId"], request.Query["sourcePlanId"],
        request.Query["targetGroupId"], request.Query["targetPlanId"],
        duplicationAdjustments);
    if (plan == null)
        return BadRequestString("Failed to duplicate plan");
    return OkObject(JsonSerializer.Serialize(plan, Handler.DefaultOptions));
}));

app.MapPost("/duplicateBucket", (HttpRequest request) => Handle(async () =>
{
    var planner = new Planner(Authorization(request));
    BucketWithDuplicationAdjustments? bucketWithDuplicationAdjustments = null;
    var body = await ReadBody(request);
    if (!string.IsNullOrEmpty(body))
        bucketWithDuplicationAdjustments = JsonSerializer.Deserialize<BucketWithDuplicationAdjustments>(body);
    if (bucketWithDuplicationAdjustments?.Bucket == null)
        return BadRequestString("Couldn't parse the expected bucket and duplication adjustments in the body");
    var bucketId = await planner.DuplicateBucket(request.Query["targetPlanId"], bucketWithDuplicationAdjustments);
    if (bucketId == null)
        return BadRequestString("Failed to duplicate bucket");
    return OkObject(bucketId);
}));

app.MapGet("/user", (HttpRequest request) => Handle(async () =>
{
    var planner = new Planner(Authorization(request));
    var idOrEmail = request.Query["userIdOrEmail"].ToString();
    var user = await planner.GetGraphUser(idOrEmail);
    if (user == null)
        return BadRequestString($"Failed to identify user by id or email {idOrEmail}");
    return OkObject(JsonSerializer.Serialize(user, Handler.DefaultOptions));
}));

app.MapMethods("/echo", new[] { "GET", "POST" }, (HttpRequest request) => Handle(async () =>
{
    var body = await ReadBody(request);
    return OkObject(body);
}));

app.Run();

// pulls the raw Authorization header (e.g. "Bearer <token>") that is forwarded to Graph
static string Authorization(HttpRequest request)
    => request.Headers.Authorization.ToString();

static async Task<string> ReadBody(HttpRequest request)
{
    using var reader = new StreamReader(request.Body);
    return await reader.ReadToEndAsync();
}

// runs a request handler and maps exceptions the same way the old Spin handler did
static async Task<IResult> Handle(Func<Task<IResult>> action)
{
    try
    {
        return await action();
    }
    catch (Exception ex)
    {
        Handler._logger.LogError("Error during request handling:");
        Handler._logger.LogError(ex.Message);
        Handler._logger.LogError(ex.StackTrace);

        var statusCode = HttpStatusCode.BadRequest;
        if (ex is HttpRequestException hre && hre.StatusCode != null)
            statusCode = (HttpStatusCode)hre.StatusCode;
        return Results.Content(ex.ToString(), "text/plain", statusCode: (int)statusCode);
    }
}

static IResult NotFound() => Results.StatusCode((int)HttpStatusCode.NotFound);

static IResult OkObject(string s) => Results.Content(s, "text/json");

// preserves the original behaviour: a "bad request" is returned as a 200 with a
// plain-text explanation rather than an error status code
static IResult BadRequestString(string s) => Results.Content(s, "text/plain");
