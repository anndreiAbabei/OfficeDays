using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;
using OfficeDays.Features.Users.Requirements;

namespace OfficeDays.Features.Users.CreateRequirement.Contracts;

public sealed record CreateRequirementRequest : IRequest
{
    [FromBody]
    public required RequirementBody Body { get; init; }
}
