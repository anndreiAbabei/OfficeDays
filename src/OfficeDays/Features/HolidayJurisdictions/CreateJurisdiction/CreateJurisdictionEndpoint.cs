using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

public sealed class CreateJurisdictionEndpoint(IRequestExecutor executor) : IHolidayJurisdictionsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", executor.Execute<CreateJurisdictionRequest>).RequireAuthorization("AdminOnly").AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
