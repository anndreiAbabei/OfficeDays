namespace OfficeDays.Features.Status.GetStatus;

public static partial class GetStatusHandlerLogging
{
    private const int EventIdStart = 3300;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "User {UserId} was not found")]
    public static partial void LogUserNotFound(this ILogger<GetStatusHandler> logger, Guid userId);
}
