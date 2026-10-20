using Microsoft.Extensions.Options;

namespace OfficeDays.Security.RateLimiting;

/// <summary>Bounded, process-local fixed windows. No denied request extends a window.</summary>
public sealed class RequestLimitStore
{
    private readonly TimeProvider _timeProvider;
    private readonly int _maxPartitions;
    private readonly Lock _gate = new Lock();
    private readonly Dictionary<string, Window> _windows = new Dictionary<string, Window>();

    public RequestLimitStore(TimeProvider timeProvider, IOptions<RequestLimits> options)
    {
        _timeProvider = timeProvider;
        _maxPartitions = options.Value.MaxPartitions;
    }

    public int Acquire(string key, RequestLimit limit)
    {
        lock (_gate)
        {
            var now = _timeProvider.GetUtcNow();
            
            if (!_windows.TryGetValue(key, out var window) || window.End <= now)
            {
                if (window is null && _windows.Count >= _maxPartitions)
                {
                    foreach (var expired in _windows.Where(x => x.Value.End <= now).Select(x => x.Key).ToArray())
                        _windows.Remove(expired);
                    // Fail closed under a high-cardinality attack, without unbounded memory growth.
                    if (_windows.Count >= _maxPartitions)
                        return Math.Max(1, (int)Math.Ceiling((_windows.Values.Min(x => x.End) - now).TotalSeconds));
                }
                window = new Window(now.AddSeconds(limit.WindowSeconds));
                _windows[key] = window;
            }
            if (window.Count >= limit.Permits)
                return Math.Max(1, (int)Math.Ceiling((window.End - now).TotalSeconds));
            
            window.Count++;
            
            return 0;
        }
    }

    private sealed class Window(DateTimeOffset end)
    {
        public DateTimeOffset End { get; } = end;
        public int Count { get; set; }
    }
}
