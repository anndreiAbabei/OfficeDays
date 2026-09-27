using OfficeDays.Features.Vacations.CreateVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.CreateVacation;

public sealed class CreateVacationEndpoint(IRequestExecutor executor) : IVacationsEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", _executor.Execute<CreateVacationRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
