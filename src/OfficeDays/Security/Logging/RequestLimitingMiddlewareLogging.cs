using OfficeDays.Security.RateLimiting;

namespace OfficeDays.Security.Logging;

internal static partial class RequestLimitingMiddlewareLogging
{
    private const int EventIdStart = 4200;

    [LoggerMessage(EventIdStart + 1, LogLevel.Error, "The endpoint binder returns the normal invalid JSON response.")]
    public static partial void LogJsonExceptionError(this ILogger<RequestLimitingMiddleware> logger, Exception exception);

    [LoggerMessage(EventIdStart + 2, LogLevel.Error, "Fail to aquire lease when rate limiting was checked")]
    public static partial void LogFailToAquireLease(this ILogger<RequestLimitingMiddleware> logger);

    [LoggerMessage(EventIdStart + 3, LogLevel.Error, "Content was bigger than max accepted size. Request size: {RequestSize} vs Allowed: {MaxAllowed})")]
    public static partial void LogContentLengthExcededAllowedSize(this ILogger<RequestLimitingMiddleware> logger, long? requestSize, long maxAllowed);

    [LoggerMessage(EventIdStart + 4, LogLevel.Error, "Body rejected from request limiter")]
    public static partial void LogRejectBodyAsync(this ILogger<RequestLimitingMiddleware> logger);
}
