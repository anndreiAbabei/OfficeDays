using OfficeDays.Domain;

namespace OfficeDays.Features.BankHolidays;

public static class BankHolidayMappings
{
    public static BankHolidayResponse ToViewModel(this BankHoliday holiday) =>
        new BankHolidayResponse(holiday.HolidayJurisdiction.Code, holiday.Date, holiday.Name);

    public static BankHoliday ToEntity(this BankHolidayRequest request, HolidayJurisdiction jurisdiction) =>
        new BankHoliday
        {
            Id = Guid.NewGuid(), HolidayJurisdictionId = jurisdiction.Id, HolidayJurisdiction = jurisdiction,
            Date = request.Date, Name = request.Name!.Trim()
        };
}
