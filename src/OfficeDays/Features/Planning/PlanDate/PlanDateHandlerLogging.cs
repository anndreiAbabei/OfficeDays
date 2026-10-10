namespace OfficeDays.Features.Planning.PlanDate;

public static partial class PlanDateHandlerLogging
{
    private const int EventIdStart = 3600;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "Office plan planned for user {UserId} on {Date}")]
    public static partial void LogPlanChanged(this ILogger<PlanDateHandler> logger, Guid userId, DateOnly date);

    [LoggerMessage(EventIdStart + 2, LogLevel.Debug, "Resolved concurrent duplicate office plan for user {UserId} on {Date}")]
    public static partial void LogResolvedConcurrentUpdate(this ILogger<PlanDateHandler> logger, Exception exception, Guid userId, DateOnly date);
}
