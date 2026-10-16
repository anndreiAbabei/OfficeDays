using OfficeDays.Features.Reports.GetReport.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Reports.GetReport;

public sealed class GetReportEndpoint(IRequestExecutor executor) : IReportsEndpoint
{
    private readonly IRequestExecutor _executor = executor;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", _executor.Execute<GetReportRequest>);
    }
}
