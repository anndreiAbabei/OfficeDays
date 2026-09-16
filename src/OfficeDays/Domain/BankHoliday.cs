namespace OfficeDays.Domain;

public sealed class BankHoliday
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public required string Name { get; init; }
    public int HolidayJurisdictionId { get; init; }
    public HolidayJurisdiction HolidayJurisdiction { get; init; } = null!;
}
