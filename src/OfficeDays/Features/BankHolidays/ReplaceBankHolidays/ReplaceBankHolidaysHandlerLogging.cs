namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

public static partial class ReplaceBankHolidaysHandlerLogging
{
    private const int EventIdStart = 3000;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "Admin user {UserId} replaced bank holidays for {JurisdictionCode} in {Year} with {HolidayCount} entries")]
    public static partial void LogHolidaysReplaced(this ILogger<ReplaceBankHolidaysHandler> logger, Guid userId, string jurisdictionCode, int year, int holidayCount);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "Holiday jurisdiction {JurisdictionCode} was not found")]
    public static partial void LogJurisdictionNotFound(this ILogger<ReplaceBankHolidaysHandler> logger, string jurisdictionCode);
}
