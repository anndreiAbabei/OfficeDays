namespace OfficeDays.Features.Users.GetRequirements;

public static partial class GetRequirementsHandlerLogging
{
    private const int EventIdStart = 4400;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "User {UserId} was not found")]
    public static partial void LogUserNotFound(this ILogger<GetRequirementsHandler> logger, Guid userId);
}
