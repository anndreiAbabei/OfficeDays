namespace OfficeDays.Features.Vacations.CreateVacation.Contracts;

public sealed record CreateVacationResponse(Guid Id, DateOnly From, DateOnly To, DateTimeOffset CreatedAt);
