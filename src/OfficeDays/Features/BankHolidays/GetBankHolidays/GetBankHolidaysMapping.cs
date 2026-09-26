using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays;

public static class GetBankHolidaysMapping
{
    public static GetBankHolidaysResponse ToViewModel(this BankHoliday holiday) =>
        new GetBankHolidaysResponse(holiday.HolidayJurisdiction.Code, holiday.Date, holiday.Name);

}
