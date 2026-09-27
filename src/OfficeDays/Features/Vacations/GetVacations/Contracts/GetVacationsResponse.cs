using System.Collections;

namespace OfficeDays.Features.Vacations.GetVacations.Contracts;

public sealed record GetVacationsItem(Guid Id, DateOnly From, DateOnly To, DateTimeOffset CreatedAt);
public sealed record GetVacationsResponse : IEnumerable<GetVacationsItem>
{
    private readonly IEnumerable<GetVacationsItem> _items;
    
    public GetVacationsResponse(IEnumerable<GetVacationsItem> items)
    {
        _items = items;
    }
    
    public IEnumerator<GetVacationsItem> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
