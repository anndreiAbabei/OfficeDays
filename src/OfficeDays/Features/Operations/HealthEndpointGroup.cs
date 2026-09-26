using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Operations;

public interface IHealthEndpoint : IEndpoint;

public sealed class HealthEndpointGroup : IRootEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints) => endpoints.MapEndpoints<IHealthEndpoint>("health")
                                                                      .AllowAnonymous();
}
