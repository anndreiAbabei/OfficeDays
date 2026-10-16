namespace OfficeDays.Features.Users.UpdateRequirement;

public static partial class UpdateRequirementHandlerLogging
{
    private const int EventIdStart = 4500;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "Attendance requirement {RequirementId} was not found for user {UserId}")]
    public static partial void LogRequirementNotFound(this ILogger<UpdateRequirementHandler> logger, Guid requirementId, Guid userId);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "Rejected a change to the initial attendance requirement {RequirementId} for user {UserId}")]
    public static partial void LogBaselineChangeRejected(this ILogger<UpdateRequirementHandler> logger, Guid requirementId, Guid userId);

    [LoggerMessage(EventIdStart + 3, LogLevel.Warning, "An attendance requirement already exists for user {UserId} starting {StartingFrom}")]
    public static partial void LogDuplicateRequirement(this ILogger<UpdateRequirementHandler> logger, Exception exception, Guid userId, DateOnly startingFrom);
}
