using OfficeDays.Features.Users.UpdateUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Users.UpdateUser;

public sealed class UpdateUserEndpoint(IRequestExecutor executor) : IUsersEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("me", _executor.Execute<UpdateUserRequest>)
                 .RequireAuthorization()
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
