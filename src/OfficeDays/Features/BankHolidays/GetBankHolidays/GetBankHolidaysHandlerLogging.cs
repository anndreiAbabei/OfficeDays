namespace OfficeDays.Features.BankHolidays.GetBankHolidays;

public static partial class GetBankHolidaysHandlerLogging
{
    private const int EventIdStart = 3200;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "Holiday jurisdiction {JurisdictionCode} was not found")]
    public static partial void LogJurisdictionNotFound(this ILogger<GetBankHolidaysHandler> logger, string jurisdictionCode);
}
