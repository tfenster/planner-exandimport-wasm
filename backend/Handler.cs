using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace planner_exandimport_wasm;

// Holds process-wide state shared by the request handlers, the Planner and the
// Graph helper. Previously this type also owned the Fermyon Spin HTTP entry
// point; with the move to a plain ASP.NET Core host the request pipeline lives
// in Program.cs and this only keeps the shared logger and serializer options.
public static class Handler
{
    public static ILogger _logger = null!;

    public static JsonSerializerOptions DefaultOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
}
