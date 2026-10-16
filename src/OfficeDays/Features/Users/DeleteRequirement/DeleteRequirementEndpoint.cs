using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Features.Users.DeleteRequirement.Contracts;

namespace OfficeDays.Features.Users.DeleteRequirement;

public sealed class DeleteRequirementEndpoint(IRequestExecutor executor) : IUsersEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("me/requirements/{id:guid}", _executor.Execute<DeleteRequirementRequest>)
                 .RequireAuthorization()
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
