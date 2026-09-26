namespace OfficeDays.Features.Vacations.CreateVacation;

public static partial class CreateVacationHandlerLogging
{
    [LoggerMessage(2501, LogLevel.Information, "User {UserId} added vacation {VacationId} from {FromDate} to {ToDate}")]
    public static partial void LogVacationCreated(this ILogger<CreateVacationHandler> logger, Guid userId, Guid vacationId, DateOnly fromDate, DateOnly toDate);
}
