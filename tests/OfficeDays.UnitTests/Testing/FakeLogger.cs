using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace OfficeDays.UnitTests.Testing;

[ExcludeFromCodeCoverage]
public sealed class FakeLogger<T> : ILogger<T>
{
    public IList<LogItem> Logs { get; } = new List<LogItem>();
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var li = new LogItem(logLevel, formatter(state, exception), exception)
        {
            Values = CreateValues(state)
        };
        Logs.Add(li);
    }

    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new Disposable();
    
    // [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "Used for assertion")]
    // [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global", Justification = "Used for assertion")]
    // public void Assert(LogLevel logLevel, string message, Exception? exception = null)
    // {
    //     Xunit.Assert.Contains(Logs, li => li.Level == logLevel &&
    //                                       li.Message == message &&
    //                                       (exception == null || li.Exception == exception));
    // }
    
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "Used for assertion")]
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global", Justification = "Used for assertion")]
    public void Assert(LogLevel logLevel, FormattableString message, Exception? exception = null)
    {
        var args = message.GetArguments();
        foreach (var logItem in Logs)
        {
            if (logItem.Level != logLevel)
                continue;
            
            if(exception != logItem.Exception)
                continue;

            var i = 0;
            
            foreach (var value in logItem.Values)
            {
                if (value.Value.Equals(args[i]))
                    return;
                
                i++;
            }
        }
        
        Xunit.Assert.Fail($"{message.ToString()} was not found in the logs collection");
    }

    private static Dictionary<string, object> CreateValues<TState>(TState state)
    {
        if(state is IEnumerable<KeyValuePair<string, object>> values)
            return values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        return new Dictionary<string, object>();
    }
    
    [ExcludeFromCodeCoverage]
    private sealed class Disposable : IDisposable
    {
        public void Dispose() {}
    }
}

[ExcludeFromCodeCoverage]
public sealed record LogItem(LogLevel Level, string Message, Exception? Exception)
{
    public IReadOnlyDictionary<string, object> Values { get; init; } = new Dictionary<string, object>();
}