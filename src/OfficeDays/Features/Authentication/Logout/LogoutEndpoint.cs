using OfficeDays.Features.Authentication.Logout.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication.Logout;

public sealed class LogoutEndpoint(IRequestExecutor executor) : IAuthenticationEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("logout", _executor.Execute<LogoutRequest>)
                 .RequireAuthorization()
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
