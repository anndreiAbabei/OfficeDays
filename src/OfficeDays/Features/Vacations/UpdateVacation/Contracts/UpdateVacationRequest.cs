using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Vacations.UpdateVacation.Contracts;

public sealed record UpdateVacationRequest : IRequest
{
    [FromRoute(Name = "id")]
    public Guid Id { get; init; }

    [FromBody]
    public required UpdateVacationRequestBody Body { get; init; }
}
