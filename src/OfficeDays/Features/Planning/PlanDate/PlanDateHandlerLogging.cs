namespace OfficeDays.Features.Planning.PlanDate;

public static partial class PlanDateHandlerLogging
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Office plan planned for user {UserId} on {Date}")]
    public static partial void LogPlanChanged(this ILogger logger, Guid userId, DateOnly date);
}
