using OfficeDays.Features.Automation.GetIosAutomation.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Automation.GetIosAutomation;

public sealed class GetIosAutomationEndpoint(IRequestExecutor executor) : IAutomationEndpoint
{
    private readonly IRequestExecutor _executor = executor;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("ios", _executor.Execute<GetIosAutomationRequest>);
    }
}
