using OfficeDays.Features.Tokens.GetTokens.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Tokens.GetTokens;

public static class GetTokensMapping
{
    public static GetTokensResponse ToViewModel(this IEnumerable<ApiToken> tokens)
    {
        return new GetTokensResponse(tokens.Select(ToViewModel));
    }

    public static GetTokensItem ToViewModel(this ApiToken token)
    {
        return new GetTokensItem(token.Id, token.Name, token.CreatedAt, token.LastUsedAt, token.RevokedAt, token.RevokedAt is null ? "Active" : "Revoked");
    }
}
