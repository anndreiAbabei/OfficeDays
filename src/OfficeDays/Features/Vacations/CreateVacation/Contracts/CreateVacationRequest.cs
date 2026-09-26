using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Vacations.CreateVacation.Contracts;

public sealed record CreateVacationRequest : IRequest
{
    [FromBody]
    public CreateVacationRequestBody Body { get; init; } = default!;
}
