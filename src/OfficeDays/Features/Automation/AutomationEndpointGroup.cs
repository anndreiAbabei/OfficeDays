using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Automation;

public interface IAutomationEndpoint : IEndpoint;

public sealed class AutomationEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IAutomationEndpoint>("automation")
                 .RequireAuthorization();
    }
}
