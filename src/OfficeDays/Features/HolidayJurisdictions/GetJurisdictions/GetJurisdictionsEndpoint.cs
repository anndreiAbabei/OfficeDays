using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.HolidayJurisdictions.GetJurisdictions;

public sealed class GetJurisdictionsEndpoint(IRequestExecutor executor) : IHolidayJurisdictionsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", executor.Execute<GetJurisdictionsRequest>).AllowAnonymous();
    }
}
