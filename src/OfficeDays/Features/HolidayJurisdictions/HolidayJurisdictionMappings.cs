using OfficeDays.Domain;

namespace OfficeDays.Features.HolidayJurisdictions;

public static class HolidayJurisdictionMappings
{
    public static HolidayJurisdictionResponse ToViewModel(this HolidayJurisdiction jurisdiction) =>
        new HolidayJurisdictionResponse(jurisdiction.Code, jurisdiction.Name);
}
