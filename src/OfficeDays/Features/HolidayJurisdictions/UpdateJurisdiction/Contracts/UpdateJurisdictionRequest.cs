using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;

public sealed record UpdateJurisdictionRequest : IRequest
{
    [FromRoute(Name = "code")]
    public required string Code { get; init; }

    [FromBody]
    public required UpdateJurisdictionRequestBody Body { get; init; }
}
