using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Status.GetStatus;

public sealed class GetStatusEndpoint(IRequestExecutor executor) : IStatusEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", executor.Execute<GetStatusRequest>);
    }
}
