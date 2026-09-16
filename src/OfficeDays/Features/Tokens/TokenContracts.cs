namespace OfficeDays.Features.Tokens;

public sealed record CreateTokenRequest(string? Name);
public sealed record TokenResponse(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? LastUsedAt, DateTimeOffset? RevokedAt, string Status);
public sealed record CreatedTokenResponse(Guid Id, string Name, string Token, DateTimeOffset CreatedAt);
