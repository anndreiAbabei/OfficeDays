using OfficeDays.Features.Vacations.RemoveVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.RemoveVacation;

public sealed class RemoveVacationEndpoint(IRequestExecutor executor) : IVacationsEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{id:guid}", _executor.Execute<RemoveVacationRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
