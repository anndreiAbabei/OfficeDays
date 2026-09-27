namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

public static partial class CreateJurisdictionHandlerLogging
{
    private const int EventIdStart = 2700;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "A duplicate holiday jurisdiction was attempted for {JurisdictionCode}")]
    public static partial void LogDuplicateJurisdiction(this ILogger<CreateJurisdictionHandler> logger, string jurisdictionCode);

    [LoggerMessage(EventIdStart + 2, LogLevel.Information, "Created holiday jurisdiction {JurisdictionCode}")]
    public static partial void LogJurisdictionCreated(this ILogger<CreateJurisdictionHandler> logger, string jurisdictionCode);
}
