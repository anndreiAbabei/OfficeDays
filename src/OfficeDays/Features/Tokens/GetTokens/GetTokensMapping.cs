using OfficeDays.Features.Tokens.GetTokens.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Tokens.GetTokens;

public static class GetTokensMapping
{
    public static GetTokensResponse ToViewModel(this ApiToken token) => new GetTokensResponse(token.Id, token.Name, token.CreatedAt, token.LastUsedAt, token.RevokedAt, token.RevokedAt is null ? "Active" : "Revoked");

}
