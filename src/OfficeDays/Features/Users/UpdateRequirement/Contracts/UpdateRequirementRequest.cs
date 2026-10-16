using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users.UpdateRequirement.Contracts;

public sealed record UpdateRequirementRequest : IRequest
{
    [FromRoute]
    public Guid Id { get; init; }
    
    [FromBody]
    public required UpdateRequirementRequestBody Body { get; init; }
}
