using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Tokens.GetTokens.Contracts;

public sealed record GetTokensItem(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? LastUsedAt, DateTimeOffset? RevokedAt, string Status);

public sealed record GetTokensResponse : CollectionResponse<GetTokensItem>
{
    public GetTokensResponse(IEnumerable<GetTokensItem> items) 
        : base(items)
    {
    }
}
