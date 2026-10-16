using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Reports;

public interface IReportsEndpoint : IEndpoint;

public sealed class ReportsEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IReportsEndpoint>("reports")
                 .RequireAuthorization();
    }
}
