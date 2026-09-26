using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.HolidayJurisdictions.GetJurisdictions;

public static class GetJurisdictionsMapping
{
    public static GetJurisdictionsResponse ToViewModel(this IEnumerable<HolidayJurisdiction> jurisdictions) => new GetJurisdictionsResponse(jurisdictions.Select(ToViewModel));
    
    public static GetJurisdictionsItem ToViewModel(this HolidayJurisdiction jurisdiction) => new GetJurisdictionsItem(jurisdiction.Code, jurisdiction.Name);
}
