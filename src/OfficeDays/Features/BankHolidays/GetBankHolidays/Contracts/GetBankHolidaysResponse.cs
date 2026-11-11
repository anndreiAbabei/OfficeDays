using OfficeDays.Infrastructure;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;

public sealed record GetBankHolidayItem(string CountryCode, DateOnly Date, string Name);

public sealed record GetBankHolidaysResponse : CollectionResponse<GetBankHolidayItem>
{
    public GetBankHolidaysResponse(IEnumerable<GetBankHolidayItem> items) 
        : base(items)
    {
    }
}


