using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Features.Users.CreateRequirement.Contracts;

namespace OfficeDays.Features.Users.CreateRequirement;

public sealed class CreateRequirementEndpoint(IRequestExecutor executor) : IUsersEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("me/requirements", _executor.Execute<CreateRequirementRequest>)
                 .RequireAuthorization()
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
