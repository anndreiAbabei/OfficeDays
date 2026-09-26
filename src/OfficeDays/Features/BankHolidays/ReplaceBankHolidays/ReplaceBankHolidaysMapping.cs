using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

public static class ReplaceBankHolidaysMapping
{
    public static ReplaceBankHolidaysResponse ToViewModel(this IEnumerable<BankHoliday> holidays) =>
        new ReplaceBankHolidaysResponse(holidays.Select(ToViewModel));

    public static ReplaceBankHolidaysItem ToViewModel(this BankHoliday holiday) => 
        new ReplaceBankHolidaysItem(holiday.HolidayJurisdiction.Code, holiday.Date, holiday.Name);

    public static BankHoliday ToEntity(this BankHolidayItem request, HolidayJurisdiction jurisdiction) => new BankHoliday
    {
        Id = Guid.NewGuid(), 
        HolidayJurisdictionId = jurisdiction.Id, 
        HolidayJurisdiction = jurisdiction,
        Date = request.Date, 
        Name = request.Name.Trim()
    };
}
