using System.Collections;

namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;

public sealed record ReplaceBankHolidaysItem(string CountryCode, DateOnly Date, string Name);

public sealed record ReplaceBankHolidaysResponse : IEnumerable<ReplaceBankHolidaysItem>
{
    private readonly IEnumerable<ReplaceBankHolidaysItem> _items;
    
    public ReplaceBankHolidaysResponse(IEnumerable<ReplaceBankHolidaysItem> items)
    {
        _items = [.. items];
    }
    
    public IEnumerator<ReplaceBankHolidaysItem> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
