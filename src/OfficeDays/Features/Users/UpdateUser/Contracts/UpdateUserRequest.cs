using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users.UpdateUser.Contracts;

public sealed record UpdateUserRequest : IRequest
{
    [FromBody]
    public required UpdateUserRequestBody Body { get; init; }
}
