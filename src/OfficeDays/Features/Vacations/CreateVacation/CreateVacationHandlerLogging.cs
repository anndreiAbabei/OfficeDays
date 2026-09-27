namespace OfficeDays.Features.Vacations.CreateVacation;

public static partial class CreateVacationHandlerLogging
{
    private const int EventIdStart = 2500;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "User {UserId} added vacation {VacationId} from {FromDate} to {ToDate}")]
    public static partial void LogVacationCreated(this ILogger<CreateVacationHandler> logger, Guid userId, Guid vacationId, DateOnly fromDate, DateOnly toDate);
}
