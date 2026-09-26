namespace OfficeDays.Features.Tokens.GetTokens.Contracts;

public sealed record GetTokensResponse(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? LastUsedAt, DateTimeOffset? RevokedAt, string Status);
