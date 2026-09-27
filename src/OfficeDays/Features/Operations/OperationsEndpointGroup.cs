using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Operations;

public interface IOperationsEndpoint : IEndpoint;

public sealed class OperationsEndpointGroup : IRootEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints) => endpoints.MapEndpoints<IOperationsEndpoint>("api")
                                                                      .AllowAnonymous();
}
