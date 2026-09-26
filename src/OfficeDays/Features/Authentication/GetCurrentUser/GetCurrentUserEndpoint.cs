using OfficeDays.Features.Authentication.GetCurrentUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication.GetCurrentUser;

public sealed class GetCurrentUserEndpoint(IRequestExecutor executor) : IAuthenticationEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("me", executor.Execute<GetCurrentUserRequest>).RequireAuthorization();
    }
}
