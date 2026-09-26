using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Vacations.RemoveVacation.Contracts;

public sealed record RemoveVacationRequest : IRequest
{
    [FromRoute(Name = "id")]
    public Guid Id { get; init; }
}
