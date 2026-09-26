using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions;

public interface IHolidayJurisdictionsEndpoint : IEndpoint;

public sealed class HolidayJurisdictionsEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IHolidayJurisdictionsEndpoint>("holiday-jurisdictions");
    }
}
