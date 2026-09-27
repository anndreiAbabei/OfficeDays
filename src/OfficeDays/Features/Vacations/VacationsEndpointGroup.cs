using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Vacations;

public interface IVacationsEndpoint : IEndpoint;

public sealed class VacationsEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IVacationsEndpoint>("vacations").RequireAuthorization();
    }
}
