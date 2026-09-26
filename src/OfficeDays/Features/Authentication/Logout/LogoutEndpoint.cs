using OfficeDays.Features.Authentication.Logout.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication.Logout;

public sealed class LogoutEndpoint(IRequestExecutor executor) : IAuthenticationEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("logout", executor.Execute<LogoutRequest>).RequireAuthorization().AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
