using OfficeDays.Domain;

namespace OfficeDays.Features.BankHolidays;

public static class BankHolidayMappings
{
    public static BankHolidayResponse ToViewModel(this BankHoliday holiday) => new BankHolidayResponse(holiday.Date, holiday.Name);

    public static BankHoliday ToEntity(this BankHolidayRequest request) => new BankHoliday { Id = Guid.NewGuid(), Date = request.Date, Name = request.Name!.Trim() };
}
