// https://github.com/christophwille/SpinHello/blob/abef0de9810abe2894f1906e5dd740e672f19b34/src/Handler/SpinLogger.cs

using Microsoft.Extensions.Logging;

namespace planner_exandimport_wasm
{
    // https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
    internal class SpinLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (null != exception)
            {
                Console.Error.WriteLine($"  {formatter(state, exception)}");
                Console.Error.WriteLine(exception.ToString());
            }
            else
            {
                Console.WriteLine($"  {formatter(state, exception)}");
            }
        }
    }
}