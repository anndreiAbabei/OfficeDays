namespace OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;

public static partial class DeleteJurisdictionHandlerLogging
{
    private const int EventIdStart = 2900;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "Deleted holiday jurisdiction {JurisdictionCode}")]
    public static partial void LogJurisdictionDeleted(this ILogger<DeleteJurisdictionHandler> logger, string jurisdictionCode);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "Holiday jurisdiction {JurisdictionCode} was not found")]
    public static partial void LogJurisdictionNotFound(this ILogger<DeleteJurisdictionHandler> logger, string jurisdictionCode);

    [LoggerMessage(EventIdStart + 3, LogLevel.Warning, "Cannot delete holiday jurisdiction {JurisdictionCode} because it is in use")]
    public static partial void LogJurisdictionInUse(this ILogger<DeleteJurisdictionHandler> logger, string jurisdictionCode);
}
