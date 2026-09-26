using OfficeDays.Features.Vacations.RemoveVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.RemoveVacation;

public sealed class RemoveVacationEndpoint(IRequestExecutor executor) : IVacationsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{id:guid}", executor.Execute<RemoveVacationRequest>).AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
