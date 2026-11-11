using System.Collections;

namespace OfficeDays.Infrastructure;

public abstract record CollectionResponse<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _items;

    protected CollectionResponse(IEnumerable<T> items)
    {
        _items = [..items];
    }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
