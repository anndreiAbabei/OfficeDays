namespace OfficeDays.Features.Planning.PlanDate;

public static partial class PlanDateHandlerLogging
{
    private const int EventIdStart = 3600;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "Office plan planned for user {UserId} on {Date}")]
    public static partial void LogPlanChanged(this ILogger<PlanDateHandler> logger, Guid userId, DateOnly date);

    [LoggerMessage(EventIdStart + 2, LogLevel.Debug, "Resolved concurrent duplicate office plan for user {UserId} on {Date}")]
    public static partial void LogResolvedConcurrentUpdate(this ILogger<PlanDateHandler> logger, Exception exception, Guid userId, DateOnly date);

    [LoggerMessage(EventIdStart + 3, LogLevel.Warning, "User {UserId} was not found")]
    public static partial void LogUserNotFound(this ILogger<PlanDateHandler> logger, Guid userId);

    [LoggerMessage(EventIdStart + 4, LogLevel.Warning, "User {UserId} cannot plan office attendance on ineligible date {Date}")]
    public static partial void LogDateIneligible(this ILogger<PlanDateHandler> logger, Guid userId, DateOnly date);
}
