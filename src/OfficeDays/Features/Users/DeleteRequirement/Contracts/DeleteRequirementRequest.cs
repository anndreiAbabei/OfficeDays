using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users.DeleteRequirement.Contracts;

public sealed record DeleteRequirementRequest : IRequest
{
    [FromRoute]
    public Guid Id { get; init; }
}
