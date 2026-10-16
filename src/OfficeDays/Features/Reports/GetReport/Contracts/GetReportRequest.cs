using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Reports.GetReport.Contracts;

public sealed record GetReportRequest : IRequest
{
    [FromQuery(Name = "year")]
    public int? Year { get; init; }

    [FromQuery(Name = "startMonth")]
    public int? StartMonth { get; init; }

    [FromQuery(Name = "endMonth")]
    public int? EndMonth { get; init; }
}
