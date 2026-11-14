namespace OfficeDays.Features.Planning.RemovePlan;

public static partial class RemovePlanHandlerLogging
{
    private const int EventIdStart = 3700;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "Office plan removed for user {UserId} on {Date}")]
    public static partial void LogPlanChanged(this ILogger<RemovePlanHandler> logger, Guid userId, DateOnly date);
    
    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "User {UserId} not found")]
    public static partial void LogUserNotFound(this ILogger<RemovePlanHandler> logger, Guid userId);

}
