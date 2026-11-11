using OfficeDays.Infrastructure;

namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;

public sealed record ReplaceBankHolidaysItem(string CountryCode, DateOnly Date, string Name);

public sealed record ReplaceBankHolidaysResponse : CollectionResponse<ReplaceBankHolidaysItem>
{
    public ReplaceBankHolidaysResponse(IEnumerable<ReplaceBankHolidaysItem> items) : base(items) {}
}
