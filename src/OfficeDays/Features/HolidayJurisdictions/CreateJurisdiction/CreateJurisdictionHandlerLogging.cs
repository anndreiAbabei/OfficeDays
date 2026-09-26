namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

public static partial class CreateJurisdictionHandlerLogging
{
    [LoggerMessage(2701, LogLevel.Warning, "A duplicate holiday jurisdiction was attempted for {JurisdictionCode}")]
    public static partial void LogDuplicateJurisdiction(this ILogger<CreateJurisdictionHandler> logger, string jurisdictionCode);

    [LoggerMessage(2702, LogLevel.Information, "Created holiday jurisdiction {JurisdictionCode}")]
    public static partial void LogJurisdictionCreated(this ILogger<CreateJurisdictionHandler> logger, string jurisdictionCode);
}
