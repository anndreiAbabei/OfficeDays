namespace OfficeDays.Security;

public static partial class CookieAntiforgeryFilterLogging
{
    private const int EventIdStart = 4100;

    [LoggerMessage(EventIdStart + 1, LogLevel.Error, "Antiforgery validation failed for {Path}")]
    public static partial void LogValidationFailed(this ILogger<CookieAntiforgeryFilter> logger, Exception exception, string path);
}
