using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Planning.RemovePlan.Contracts;

public sealed record RemovePlanRequest : IRequest
{
    [FromRoute(Name = "date")]
    public DateOnly Date { get; init; }
}
