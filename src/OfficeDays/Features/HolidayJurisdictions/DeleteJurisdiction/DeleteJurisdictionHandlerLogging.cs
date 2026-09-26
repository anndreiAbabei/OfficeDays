namespace OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;

public static partial class DeleteJurisdictionHandlerLogging
{
    [LoggerMessage(2901, LogLevel.Information, "Deleted holiday jurisdiction {JurisdictionCode}")]
    public static partial void LogJurisdictionDeleted(this ILogger<DeleteJurisdictionHandler> logger, string jurisdictionCode);
}
