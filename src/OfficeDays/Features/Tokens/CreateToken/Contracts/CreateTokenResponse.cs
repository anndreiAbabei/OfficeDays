namespace OfficeDays.Features.Tokens.CreateToken.Contracts;

public sealed record CreateTokenResponse(Guid Id, string Name, string Token, DateTimeOffset CreatedAt);
