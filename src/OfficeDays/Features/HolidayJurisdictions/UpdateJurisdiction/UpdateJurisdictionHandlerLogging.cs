namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

public static partial class UpdateJurisdictionHandlerLogging
{
    [LoggerMessage(2801, LogLevel.Information, "Updated holiday jurisdiction {JurisdictionCode}")]
    public static partial void LogJurisdictionUpdated(this ILogger<UpdateJurisdictionHandler> logger, string jurisdictionCode);
}
