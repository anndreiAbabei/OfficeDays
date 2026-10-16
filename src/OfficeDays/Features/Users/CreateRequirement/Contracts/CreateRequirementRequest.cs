using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users.CreateRequirement.Contracts;

public sealed record CreateRequirementRequest : IRequest
{
    [FromBody]
    public required CreateRequirementRequestBody Body { get; init; }
}
