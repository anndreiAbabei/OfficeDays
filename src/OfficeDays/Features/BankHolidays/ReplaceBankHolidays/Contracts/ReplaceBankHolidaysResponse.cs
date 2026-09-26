namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;

public sealed record ReplaceBankHolidaysResponse(string CountryCode, DateOnly Date, string Name);
