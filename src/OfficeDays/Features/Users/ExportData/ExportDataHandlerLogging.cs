namespace OfficeDays.Features.Users.ExportData;

public static partial class ExportDataHandlerLogging
{
    private const int EventIdStart = 4900;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "User {UserId} was not found")]
    public static partial void LogUserNotFound(this ILogger<ExportDataHandler> logger, Guid userId);
}
