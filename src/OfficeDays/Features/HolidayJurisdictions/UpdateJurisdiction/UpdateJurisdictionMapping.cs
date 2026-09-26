using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

public static class UpdateJurisdictionMapping
{
    public static UpdateJurisdictionResponse ToViewModel(this HolidayJurisdiction jurisdiction) =>
        new UpdateJurisdictionResponse(jurisdiction.Code, jurisdiction.Name);
}
