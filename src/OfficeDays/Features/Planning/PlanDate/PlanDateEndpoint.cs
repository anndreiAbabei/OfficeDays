using OfficeDays.Features.Planning.PlanDate.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Planning.PlanDate;

public sealed class PlanDateEndpoint(IRequestExecutor executor) : IPlanningEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("{date}", _executor.Execute<PlanDateRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
