namespace OfficeDays.Features.Authentication.Logout;

public static partial class LogoutHandlerLogging
{
    [LoggerMessage(2101, LogLevel.Information, "User {UserId} signed out")]
    public static partial void LogSignedOut(this ILogger<LogoutHandler> logger, Guid userId);
}
