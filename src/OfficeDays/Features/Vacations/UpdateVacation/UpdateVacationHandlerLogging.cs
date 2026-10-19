namespace OfficeDays.Features.Vacations.UpdateVacation;

public static partial class UpdateVacationHandlerLogging
{
    private const int EventIdStart = 4700;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "User {UserId} updated vacation {VacationId} from {FromDate} to {ToDate}")]
    public static partial void LogVacationUpdated(this ILogger<UpdateVacationHandler> logger, Guid userId, Guid vacationId, DateOnly fromDate, DateOnly toDate);

    [LoggerMessage(EventIdStart + 2, LogLevel.Warning, "Vacation {VacationId} not found for user {UserId}")]
    public static partial void LogVacationNotFound(this ILogger<UpdateVacationHandler> logger, Guid vacationId, Guid userId);
}
