namespace OfficeDays.Features.Authentication.Login;

public static partial class LoginHandlerLogging
{
    private const int EventIdStart = 2000;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "Failed login attempt for username {Username} from {RemoteIpAddress}")]
    public static partial void LogFailedLogin(this ILogger<LoginHandler> logger, string? username, System.Net.IPAddress? remoteIpAddress);

    [LoggerMessage(EventIdStart + 2, LogLevel.Information, "User {UserId} signed in")]
    public static partial void LogSignedIn(this ILogger<LoginHandler> logger, Guid userId);
}
