using System.Collections;

namespace OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;

public sealed record GetJurisdictionsItem(string Code, string Name);
public sealed record GetJurisdictionsResponse : IEnumerable<GetJurisdictionsItem>
{
    private readonly IEnumerable<GetJurisdictionsItem> _items;
    
    public GetJurisdictionsResponse(IEnumerable<GetJurisdictionsItem> items)
    {
        _items = items;
    }

    public IEnumerator<GetJurisdictionsItem> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
