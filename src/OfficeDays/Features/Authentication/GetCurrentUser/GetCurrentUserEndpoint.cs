using OfficeDays.Features.Authentication.GetCurrentUser.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Authentication.GetCurrentUser;

public sealed class GetCurrentUserEndpoint(IRequestExecutor executor) : IAuthenticationEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("me", _executor.Execute<GetCurrentUserRequest>)
                 .RequireAuthorization();
    }
}
