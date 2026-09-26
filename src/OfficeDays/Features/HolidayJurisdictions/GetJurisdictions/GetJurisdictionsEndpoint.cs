using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions.GetJurisdictions;

public sealed class GetJurisdictionsEndpoint(IRequestExecutor executor) : IHolidayJurisdictionsEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", _executor.Execute<GetJurisdictionsRequest>)
                 .AllowAnonymous();
    }
}
