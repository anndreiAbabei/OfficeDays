using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Planning.GetPlanning.Contracts;

public sealed record GetPlanningRequest : IRequest
{
    [FromQuery(Name = "year")]
    public int Year { get; init; }
    
    [FromQuery(Name = "month")]
    public int Month { get; init; }
}
