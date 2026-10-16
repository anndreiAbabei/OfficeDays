using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementEndpoint(IRequestExecutor executor) : IUsersEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("me/requirements/{id:guid}", _executor.Execute<UpdateRequirementRequest>)
                 .RequireAuthorization()
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
