using OfficeDays.Features.Authentication.Login.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication.Login;

public sealed class LoginEndpoint(IRequestExecutor executor) : IAuthenticationEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("login", executor.Execute<LoginRequest>).AllowAnonymous();
    }
}
