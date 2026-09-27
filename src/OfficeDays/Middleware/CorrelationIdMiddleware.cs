using System.Diagnostics;

namespace OfficeDays.Middleware;

public sealed partial class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    
    public const string HeaderName = "X-Correlation-ID";
    
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var supplied = context.Request.Headers[HeaderName];
        
        string? correlationId = null;
        
        if(supplied.Count == 1 && !string.IsNullOrWhiteSpace(supplied[0]))
            correlationId = supplied[0];
        
        if(string.IsNullOrWhiteSpace(correlationId))
            correlationId = Guid.NewGuid().ToString("N");

        // Set immediately and again just before sending: exception handling can clear headers.
        context.Response.Headers[HeaderName] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });

        var started = Stopwatch.GetTimestamp();
        
        await _next(context);
        
        LogRequestCompleted(_logger, context.Request.Method, context.Request.Path.Value ?? "/", context.Response.StatusCode, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
    }

    [LoggerMessage(200, LogLevel.Information, "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms")]
    private static partial void LogRequestCompleted(ILogger logger, string method, string path, int statusCode, double elapsedMilliseconds);
}
