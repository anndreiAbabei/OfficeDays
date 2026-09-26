using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;

public sealed record DeleteJurisdictionRequest : IRequest
{
    [FromRoute(Name = "code")]
    public required string Code { get; init; }
}
