using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users.UpdateUser.Contracts;

public sealed record UpdateUserRequest : IRequest
{
    [FromBody]
    public UpdateUserRequestBody Body { get; init; } = default!;
}
