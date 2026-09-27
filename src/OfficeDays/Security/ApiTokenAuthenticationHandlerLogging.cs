using System.Net;

namespace OfficeDays.Security;

public static partial class ApiTokenAuthenticationHandlerLogging
{
    private const int EventIdStart = 4000;

    [LoggerMessage(EventIdStart + 1, LogLevel.Debug, "No bearer credentials were supplied for API token authentication")]
    public static partial void LogNoBearerCredentials(this ILogger<ApiTokenAuthenticationHandler> logger);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "Rejected an empty bearer token from {RemoteIpAddress}")]
    public static partial void LogMissingToken(this ILogger<ApiTokenAuthenticationHandler> logger, IPAddress? remoteIpAddress);

    [LoggerMessage(EventIdStart + 3, LogLevel.Warning, "Rejected an invalid API token from {RemoteIpAddress}")]
    public static partial void LogInvalidToken(this ILogger<ApiTokenAuthenticationHandler> logger, IPAddress? remoteIpAddress);

    [LoggerMessage(EventIdStart + 4, LogLevel.Warning, "Rejected revoked API token {TokenId} from {RemoteIpAddress}")]
    public static partial void LogRevokedToken(this ILogger<ApiTokenAuthenticationHandler> logger, Guid tokenId, IPAddress? remoteIpAddress);

    [LoggerMessage(EventIdStart + 5, LogLevel.Debug, "API token {TokenId} authenticated user {UserId}")]
    public static partial void LogTokenAuthenticated(this ILogger<ApiTokenAuthenticationHandler> logger, Guid tokenId, Guid userId);
}
