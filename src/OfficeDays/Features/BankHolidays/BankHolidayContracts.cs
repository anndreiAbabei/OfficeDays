namespace OfficeDays.Features.BankHolidays;

public sealed record BankHolidayRequest(DateOnly Date, string? Name);
public sealed record BankHolidayResponse(DateOnly Date, string Name);
