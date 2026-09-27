namespace OfficeDays.Features.Authentication.Logout;

public static partial class LogoutHandlerLogging
{
    private const int EventIdStart = 2100;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "User {UserId} signed out")]
    public static partial void LogSignedOut(this ILogger<LogoutHandler> logger, Guid userId);
}
