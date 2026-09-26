using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users.CreateUser;

public sealed class CreateUserEndpoint(IRequestExecutor executor) : IUsersEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", _executor.Execute<CreateUserRequest>)
                 .AllowAnonymous();
    }
}
