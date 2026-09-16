using OfficeDays.Domain;

namespace OfficeDays.Features.Tokens;

public static class TokenMappings
{
    public static TokenResponse ToViewModel(this ApiToken token) => new TokenResponse(token.Id, token.Name, token.CreatedAt, token.LastUsedAt, token.RevokedAt, token.RevokedAt is null ? "Active" : "Revoked");

    public static CreatedTokenResponse ToCreatedViewModel(this ApiToken token, string rawToken) => new CreatedTokenResponse(token.Id, token.Name, rawToken, token.CreatedAt);
}
