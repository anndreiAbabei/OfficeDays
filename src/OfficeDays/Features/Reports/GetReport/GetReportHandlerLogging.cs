namespace OfficeDays.Features.Reports.GetReport;

public static partial class GetReportHandlerLogging
{
    private const int EventIdStart = 4600;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "User {UserId} was not found")]
    public static partial void LogUserNotFound(this ILogger<GetReportHandler> logger, Guid userId);

    [LoggerMessage(EventIdStart + 2, LogLevel.Information, "Generated attendance report for user {UserId}, year {Year}, months {StartMonth} to {EndMonth}, with {MonthCount} rows")]
    public static partial void LogReportGenerated(this ILogger<GetReportHandler> logger, Guid userId, int year, int startMonth, int endMonth, int monthCount);
}
