namespace OfficeDays.Features.Vacations.GetVacations.Contracts;

public sealed record GetVacationsResponse(Guid Id, DateOnly From, DateOnly To, DateTimeOffset CreatedAt);
