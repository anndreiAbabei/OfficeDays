using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

public sealed class UpdateJurisdictionEndpoint(IRequestExecutor executor) : IHolidayJurisdictionsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("{code}", executor.Execute<UpdateJurisdictionRequest>).RequireAuthorization("AdminOnly").AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
