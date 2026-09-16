namespace OfficeDays.Features.Vacations;

public sealed record CreateVacationRequest(DateOnly From, DateOnly To);
public sealed record VacationResponse(Guid Id, DateOnly From, DateOnly To, DateTimeOffset CreatedAt);
