using OfficeDays.Features.Vacations.GetVacations.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.GetVacations;

public sealed class GetVacationsEndpoint(IRequestExecutor executor) : IVacationsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", executor.Execute<GetVacationsRequest>);
    }
}
