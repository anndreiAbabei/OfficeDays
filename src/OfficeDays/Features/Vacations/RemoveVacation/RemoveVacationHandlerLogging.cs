namespace OfficeDays.Features.Vacations.RemoveVacation;

public static partial class RemoveVacationHandlerLogging
{
    private const int EventIdStart = 2600;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "User {UserId} removed vacation {VacationId}")]
    public static partial void LogVacationRemoved(this ILogger<RemoveVacationHandler> logger, Guid userId, Guid vacationId);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "Vacation {VacationId} was not found for user {UserId}")]
    public static partial void LogVacationNotFound(this ILogger<RemoveVacationHandler> logger, Guid vacationId, Guid userId);
}
