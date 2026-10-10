using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Planning.PlanDate.Contracts;

public sealed record PlanDateRequest : IRequest
{
    [FromRoute(Name = "date")]
    public DateOnly Date { get; init; }
}
