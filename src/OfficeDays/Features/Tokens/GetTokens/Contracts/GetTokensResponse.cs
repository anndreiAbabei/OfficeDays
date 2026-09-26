using System.Collections;

namespace OfficeDays.Features.Tokens.GetTokens.Contracts;

public sealed record GetTokensItem(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? LastUsedAt, DateTimeOffset? RevokedAt, string Status);

public sealed record GetTokensResponse : IEnumerable<GetTokensItem>
{
    private readonly IEnumerable<GetTokensItem> _items;
    
    public GetTokensResponse(IEnumerable<GetTokensItem> items)
    {
        _items = items;
    }
    public IEnumerator<GetTokensItem> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
