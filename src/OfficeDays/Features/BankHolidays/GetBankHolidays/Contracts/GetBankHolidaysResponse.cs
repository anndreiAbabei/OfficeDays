namespace OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;

public sealed record GetBankHolidaysResponse(string CountryCode, DateOnly Date, string Name);
