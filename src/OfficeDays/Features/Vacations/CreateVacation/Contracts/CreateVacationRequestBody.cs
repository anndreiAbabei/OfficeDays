namespace OfficeDays.Features.Vacations.CreateVacation.Contracts;

public sealed record CreateVacationRequestBody(DateOnly From, DateOnly To);
