using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
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
            EventId = eventId,
            Values = CreateValues(state)
        };
        Logs.Add(li);
    }

    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new Disposable();
    
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "Used for assertion")]
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global", Justification = "Used for assertion")]
    public void Assert(LogLevel logLevel, FormattableString message, Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        foreach (var logItem in Logs)
        {
            if (logItem.Level != logLevel || logItem.Exception != exception)
                continue;

            if (!logItem.Values.TryGetValue("{OriginalFormat}", out var originalFormat) ||
                originalFormat is not string template)
                continue;

            var format = new StringBuilder();
            var arguments = new List<object?>();
            var validTemplate = true;
            for (var i = 0; i < template.Length; i++)
            {
                var character = template[i];
                // Keep escaped braces intact in the composite format.
                if ((character == '{' || character == '}') &&
                    i + 1 < template.Length && template[i + 1] == character)
                {
                    format.Append(character).Append(character);
                    i++;
                    continue;
                }

                if (character != '{')
                {
                    format.Append(character);
                    continue;
                }

                var end = template.IndexOf('}', i + 1);
                if (end < 0)
                {
                    validTemplate = false;
                    break;
                }

                var placeholder = template[(i + 1)..end];
                var suffix = placeholder.IndexOfAny([',', ':']);
                var name = suffix < 0 ? placeholder : placeholder[..suffix];
                if (!logItem.Values.TryGetValue(name, out var value))
                {
                    validTemplate = false;
                    break;
                }

                format.Append('{').Append(arguments.Count.ToString(CultureInfo.InvariantCulture));
                if (suffix >= 0)
                    format.Append(placeholder[suffix..]);
                format.Append('}');
                arguments.Add(value);
                i = end;
            }

            if (validTemplate && format.ToString() == message.Format && arguments.SequenceEqual(message.GetArguments()))
                return;
        }

        Xunit.Assert.Fail($"Expected {logLevel} log with format '{message.Format}' and matching arguments and exception was not found.");
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
    public EventId EventId { get; init; }
    public IReadOnlyDictionary<string, object> Values { get; init; } = new Dictionary<string, object>();
}