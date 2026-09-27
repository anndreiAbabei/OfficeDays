namespace OfficeDays.Features.Users.UpdateUser;

public static partial class UpdateUserHandlerLogging
{
    private const int EventIdStart = 3400;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "User {UserId} was not found")]
    public static partial void LogUserNotFound(this ILogger<UpdateUserHandler> logger, Guid userId);
}
