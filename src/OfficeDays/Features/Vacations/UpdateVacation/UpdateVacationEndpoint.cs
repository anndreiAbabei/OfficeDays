using OfficeDays.Features.Vacations.UpdateVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.UpdateVacation;

public sealed class UpdateVacationEndpoint(IRequestExecutor executor) : IVacationsEndpoint
{
    private readonly IRequestExecutor _executor = executor;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("{id:guid}", _executor.Execute<UpdateVacationRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
