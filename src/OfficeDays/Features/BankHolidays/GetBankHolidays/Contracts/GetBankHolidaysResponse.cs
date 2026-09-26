using System.Collections;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;

public sealed record GetBankHolidayItem(string CountryCode, DateOnly Date, string Name);

public sealed record GetBankHolidaysResponse : IEnumerable<GetBankHolidayItem>
{
    private readonly IEnumerable<GetBankHolidayItem> _items;
    
    public GetBankHolidaysResponse(IEnumerable<GetBankHolidayItem> items)
    {
        _items = [.. items];
    }
    
    public IEnumerator<GetBankHolidayItem> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
    
    
