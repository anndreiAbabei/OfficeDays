using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Users.CreateUser;

public sealed class CreateUserEndpoint(IRequestExecutor executor) : IUsersEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", executor.Execute<CreateUserRequest>).AllowAnonymous();
    }
}
