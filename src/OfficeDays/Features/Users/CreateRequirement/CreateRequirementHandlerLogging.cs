namespace OfficeDays.Features.Users.CreateRequirement;

public static partial class CreateRequirementHandlerLogging
{
    private const int EventIdStart = 4200;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "User {UserId} was not found")]
    public static partial void LogUserNotFound(this ILogger<CreateRequirementHandler> logger, Guid userId);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "An attendance requirement already exists for user {UserId} starting {StartingFrom}")]
    public static partial void LogDuplicateRequirement(this ILogger<CreateRequirementHandler> logger, Exception exception, Guid userId, DateOnly startingFrom);
}
