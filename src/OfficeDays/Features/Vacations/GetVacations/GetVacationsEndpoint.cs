using OfficeDays.Features.Vacations.GetVacations.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Vacations.GetVacations;

public sealed class GetVacationsEndpoint(IRequestExecutor executor) : IVacationsEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", _executor.Execute<GetVacationsRequest>);
    }
}
