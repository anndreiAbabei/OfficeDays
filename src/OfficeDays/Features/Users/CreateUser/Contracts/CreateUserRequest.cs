using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users.CreateUser.Contracts;

public sealed record CreateUserRequest : IRequest
{
    [FromBody]
    public CreateUserRequestBody Body { get; init; } = default!;
}
