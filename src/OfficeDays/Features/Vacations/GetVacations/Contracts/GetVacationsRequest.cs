using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Vacations.GetVacations.Contracts;

public sealed record GetVacationsRequest : IRequest
{
    [FromQuery(Name = "year")]
    public int? Year { get; init; }

    [FromQuery(Name = "month")]
    public int? Month { get; init; }
}
