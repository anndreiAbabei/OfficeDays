using OfficeDays.Features.Users.ExportData.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users.ExportData;

public sealed class ExportDataEndpoint(IRequestExecutor executor) : IUsersEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("me/export", _executor.Execute<ExportDataRequest>)
                 .RequireAuthorization();
    }
}
