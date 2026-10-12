using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace OfficeDays.UnitTests.Testing;

[ExcludeFromCodeCoverage]
public sealed class FakeLogger<T> : ILogger<T>
{
    public IList<LogItem> Logs { get; } = new List<LogItem>();
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Logs.Add(new LogItem(logLevel, formatter(state, exception), exception));
    }

    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new Disposable();
    
    public void Assert(LogLevel logLevel, string message, Exception? exception = null)
    {
        Xunit.Assert.Contains(Logs, li => li.Level == logLevel && 
                                          li.Message == message && 
                                          (exception == null || li.Exception == exception));
    }
}

[ExcludeFromCodeCoverage]
public sealed record LogItem(LogLevel Level, string Message, Exception? Exception);

[ExcludeFromCodeCoverage]
file sealed class Disposable : IDisposable
{
    public void Dispose() {}
}