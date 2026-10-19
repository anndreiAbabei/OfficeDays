namespace OfficeDays.Features.Tokens.DeleteToken;

public static partial class DeleteTokenHandlerLogging
{
    private const int EventIdStart = 4800;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "Token {TokenId} was not found for user {UserId}")]
    public static partial void LogTokenNotFound(this ILogger<DeleteTokenHandler> logger, Guid tokenId, Guid userId);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "User {UserId} cannot permanently delete API token {TokenId} because it has not been revoked")]
    public static partial void LogTokenNotRevoked(this ILogger<DeleteTokenHandler> logger, Guid tokenId, Guid userId);
}
