using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;

public sealed record CreateJurisdictionRequest : IRequest
{
    [FromBody]
    public required CreateJurisdictionRequestBody Body { get; init; }
}
