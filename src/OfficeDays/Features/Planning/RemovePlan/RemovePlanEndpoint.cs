using OfficeDays.Features.Planning.RemovePlan.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Planning.RemovePlan;

public sealed class RemovePlanEndpoint(IRequestExecutor executor) : IPlanningEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{date}", _executor.Execute<RemovePlanRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
