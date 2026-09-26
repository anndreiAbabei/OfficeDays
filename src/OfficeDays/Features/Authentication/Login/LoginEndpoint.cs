using OfficeDays.Features.Authentication.Login.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Authentication.Login;

public sealed class LoginEndpoint(IRequestExecutor executor) : IAuthenticationEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("login", _executor.Execute<LoginRequest>)
                 .AllowAnonymous();
    }
}
