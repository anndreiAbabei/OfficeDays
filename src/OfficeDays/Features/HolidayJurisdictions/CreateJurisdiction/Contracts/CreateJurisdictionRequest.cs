using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;

public sealed record CreateJurisdictionRequest : IRequest
{
    [FromBody]
    public CreateJurisdictionRequestBody Body { get; init; } = default!;
}
