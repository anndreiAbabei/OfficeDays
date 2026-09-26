namespace OfficeDays.Features.Vacations.RemoveVacation;

public static partial class RemoveVacationHandlerLogging
{
    [LoggerMessage(2601, LogLevel.Information, "User {UserId} removed vacation {VacationId}")]
    public static partial void LogVacationRemoved(this ILogger<RemoveVacationHandler> logger, Guid userId, Guid vacationId);
}
