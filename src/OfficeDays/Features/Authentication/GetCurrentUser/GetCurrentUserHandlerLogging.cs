namespace OfficeDays.Features.Authentication.GetCurrentUser;

public static partial class GetCurrentUserHandlerLogging
{
    private const int EventIdStart = 3100;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "User {UserId} was not found")]
    public static partial void LogUserNotFound(this ILogger<GetCurrentUserHandler> logger, Guid userId);
}
