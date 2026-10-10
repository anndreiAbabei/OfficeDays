namespace OfficeDays.Features.Planning.GetPlanning;

public static partial class GetPlanningHandlerLogging
{
    private const int EventIdStart = 3500;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "User {UserId} was not found")]
    public static partial void LogUserNotFound(this ILogger<GetPlanningHandler> logger, Guid userId);
}
