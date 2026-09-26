using OfficeDays.Features.Vacations.CreateVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.CreateVacation;

public sealed class CreateVacationEndpoint(IRequestExecutor executor) : IVacationsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", executor.Execute<CreateVacationRequest>).AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
