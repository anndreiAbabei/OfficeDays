using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;

public sealed class DeleteJurisdictionEndpoint(IRequestExecutor executor) : IHolidayJurisdictionsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{code}", executor.Execute<DeleteJurisdictionRequest>).RequireAuthorization("AdminOnly").AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
