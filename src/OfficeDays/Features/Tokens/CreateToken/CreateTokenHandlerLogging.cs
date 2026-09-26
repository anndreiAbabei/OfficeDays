namespace OfficeDays.Features.Tokens.CreateToken;

public static partial class CreateTokenHandlerLogging
{
    [LoggerMessage(2301, LogLevel.Information, "User {UserId} created API token {TokenId} named {TokenName}")]
    public static partial void LogTokenCreated(this ILogger<CreateTokenHandler> logger, Guid userId, Guid tokenId, string tokenName);
}
