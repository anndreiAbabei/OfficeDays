namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

public static partial class UpdateJurisdictionHandlerLogging
{
    private const int EventIdStart = 2800;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "Updated holiday jurisdiction {JurisdictionCode}")]
    public static partial void LogJurisdictionUpdated(this ILogger<UpdateJurisdictionHandler> logger, string jurisdictionCode);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "Holiday jurisdiction {JurisdictionCode} was not found")]
    public static partial void LogJurisdictionNotFound(this ILogger<UpdateJurisdictionHandler> logger, string jurisdictionCode);
}
