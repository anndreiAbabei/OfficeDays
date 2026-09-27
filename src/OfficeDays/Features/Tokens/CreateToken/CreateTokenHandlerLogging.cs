namespace OfficeDays.Features.Tokens.CreateToken;

public static partial class CreateTokenHandlerLogging
{
    private const int EventIdStart = 2300;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "User {UserId} created API token {TokenId} named {TokenName}")]
    public static partial void LogTokenCreated(this ILogger<CreateTokenHandler> logger, Guid userId, Guid tokenId, string tokenName);
}
