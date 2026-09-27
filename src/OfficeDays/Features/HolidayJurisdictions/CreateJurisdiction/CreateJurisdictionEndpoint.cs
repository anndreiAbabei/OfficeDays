using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

public sealed class CreateJurisdictionEndpoint(IRequestExecutor executor) : IHolidayJurisdictionsEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", _executor.Execute<CreateJurisdictionRequest>)
                 .RequireAuthorization("AdminOnly")
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
