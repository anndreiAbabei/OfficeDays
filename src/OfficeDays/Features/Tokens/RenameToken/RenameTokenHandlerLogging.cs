namespace OfficeDays.Features.Tokens.RenameToken;

public static partial class RenameTokenHandlerLogging
{
    private const int EventIdStart = 4900;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "Token {TokenId} was not found for user {UserId}")]
    public static partial void LogTokenNotFound(this ILogger<RenameTokenHandler> logger, Guid tokenId, Guid userId);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "Rename rejected for revoked token {TokenId} owned by user {UserId}")]
    public static partial void LogRenameRejectedForRevokedToken(this ILogger<RenameTokenHandler> logger, Guid tokenId, Guid userId);
}
