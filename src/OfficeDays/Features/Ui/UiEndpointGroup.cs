using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Ui;

public interface IUiEndpoint : IEndpoint;

public sealed class UiEndpointGroup : IRootEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints) =>
        endpoints.MapEndpoints<IUiEndpoint>("").AllowAnonymous();
}
