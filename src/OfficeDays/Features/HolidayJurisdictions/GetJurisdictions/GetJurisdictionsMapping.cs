using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.HolidayJurisdictions.GetJurisdictions;

public static class GetJurisdictionsMapping
{
    public static GetJurisdictionsResponse ToViewModel(this HolidayJurisdiction jurisdiction) =>
        new GetJurisdictionsResponse(jurisdiction.Code, jurisdiction.Name);
}
