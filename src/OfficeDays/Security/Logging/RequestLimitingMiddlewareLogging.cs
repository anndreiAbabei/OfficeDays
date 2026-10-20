using OfficeDays.Security.RateLimiting;

namespace OfficeDays.Security.Logging;

internal static partial class RequestLimitingMiddlewareLogging
{
    private const int EventIdStart = 4200;
    
    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "The endpoint binder returns the normal invalid JSON response.")]
    public static partial void LogJsonExceptionError(this ILogger<RequestLimitingMiddleware> logger, Exception exception);
}
