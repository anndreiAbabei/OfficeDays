namespace OfficeDays.Features.Planning.RemovePlan;

public static partial class RemovePlanHandlerLogging
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Office plan removed for user {UserId} on {Date}")]
    public static partial void LogPlanChanged(this ILogger logger, Guid userId, DateOnly date);
}
