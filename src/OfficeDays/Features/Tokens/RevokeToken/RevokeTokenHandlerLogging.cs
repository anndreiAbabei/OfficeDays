namespace OfficeDays.Features.Tokens.RevokeToken;

public static partial class RevokeTokenHandlerLogging
{
    [LoggerMessage(2401, LogLevel.Information, "User {UserId} revoked API token {TokenId}")]
    public static partial void LogTokenRevoked(this ILogger<RevokeTokenHandler> logger, Guid userId, Guid tokenId);
}
