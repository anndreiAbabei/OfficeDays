namespace OfficeDays.Features.Users.DeleteAccount;

public static partial class DeleteAccountHandlerLogging
{
    [LoggerMessage(4601, LogLevel.Warning, "User {UserId} was not found for account deletion")]
    public static partial void LogUserNotFound(this ILogger<DeleteAccountHandler> logger, Guid userId);

    [LoggerMessage(4602, LogLevel.Warning, "Account deletion password rejected for user {UserId}")]
    public static partial void LogPasswordRejected(this ILogger<DeleteAccountHandler> logger, Guid userId);

    [LoggerMessage(4603, LogLevel.Warning, "Account deletion blocked for last administrator {UserId}")]
    public static partial void LogLastAdministrator(this ILogger<DeleteAccountHandler> logger, Guid userId);
}
