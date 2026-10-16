using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;
using OfficeDays.Features.Users.Requirements;

namespace OfficeDays.Features.Users.UpdateRequirement.Contracts;

public sealed record UpdateRequirementRequest : IRequest
{
    [FromRoute]
    public Guid Id { get; init; }
    
    [FromBody]
    public required RequirementBody Body { get; init; }
}
