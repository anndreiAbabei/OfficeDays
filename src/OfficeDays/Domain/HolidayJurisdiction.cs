namespace OfficeDays.Domain;

public sealed class HolidayJurisdiction
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; set; }
    public List<BankHoliday> BankHolidays { get; init; } = [];
    public List<User> Users { get; init; } = [];
}
