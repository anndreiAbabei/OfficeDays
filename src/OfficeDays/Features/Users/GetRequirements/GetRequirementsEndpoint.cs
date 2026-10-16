using OfficeDays.Infrastructure;
using OfficeDays.Features.Users.GetRequirements.Contracts;

namespace OfficeDays.Features.Users.GetRequirements;

public sealed class GetRequirementsEndpoint(IRequestExecutor executor) : IUsersEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("me/requirements", _executor.Execute<GetRequirementsRequest>)
                 .RequireAuthorization();
    }
}
