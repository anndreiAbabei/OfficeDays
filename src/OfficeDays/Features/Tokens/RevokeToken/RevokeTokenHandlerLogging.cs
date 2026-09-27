namespace OfficeDays.Features.Tokens.RevokeToken;

public static partial class RevokeTokenHandlerLogging
{
    private const int EventIdStart = 2400;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "User {UserId} revoked API token {TokenId}")]
    public static partial void LogTokenRevoked(this ILogger<RevokeTokenHandler> logger, Guid userId, Guid tokenId);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "Token {TokenId} was not found for user {UserId}")]
    public static partial void LogTokenNotFound(this ILogger<RevokeTokenHandler> logger, Guid tokenId, Guid userId);
}
