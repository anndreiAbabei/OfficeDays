using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;

public sealed record UpdateJurisdictionRequest : IRequest
{
    [FromRoute(Name = "code")]
    public string Code { get; init; } = default!;

    [FromBody]
    public UpdateJurisdictionRequestBody Body { get; init; } = default!;
}
