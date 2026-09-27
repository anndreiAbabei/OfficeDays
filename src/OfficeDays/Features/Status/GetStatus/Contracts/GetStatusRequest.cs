using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Status.GetStatus.Contracts;

public sealed record GetStatusRequest : IRequest
{
    [FromQuery(Name = "year")]
    public int? Year { get; init; }

    [FromQuery(Name = "month")]
    public int? Month { get; init; }
}
