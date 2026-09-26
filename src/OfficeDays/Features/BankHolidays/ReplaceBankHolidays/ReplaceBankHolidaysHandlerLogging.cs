namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

public static partial class ReplaceBankHolidaysHandlerLogging
{
    [LoggerMessage(3001, LogLevel.Information, "Admin user {UserId} replaced bank holidays for {JurisdictionCode} in {Year} with {HolidayCount} entries")]
    public static partial void LogHolidaysReplaced(this ILogger<ReplaceBankHolidaysHandler> logger, Guid userId, string jurisdictionCode, int year, int holidayCount);
}
