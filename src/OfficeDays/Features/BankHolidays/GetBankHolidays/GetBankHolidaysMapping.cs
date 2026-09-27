using OfficeDays.Domain;
using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays;

public static class GetBankHolidaysMapping
{
    public static GetBankHolidaysResponse ToViewModel(this IEnumerable<BankHoliday> holidays) => new GetBankHolidaysResponse(holidays.Select(hd => hd.ToViewModel()));

    public static GetBankHolidayItem ToViewModel(this BankHoliday holiday) => new GetBankHolidayItem(holiday.HolidayJurisdiction.Code, holiday.Date, holiday.Name);
}
