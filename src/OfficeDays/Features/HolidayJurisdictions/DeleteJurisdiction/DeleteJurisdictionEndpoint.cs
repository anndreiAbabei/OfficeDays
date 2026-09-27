using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;

public sealed class DeleteJurisdictionEndpoint(IRequestExecutor executor) : IHolidayJurisdictionsEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{code}", _executor.Execute<DeleteJurisdictionRequest>)
                 .RequireAuthorization("AdminOnly")
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
