using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

public static class CreateJurisdictionMapping
{
    public static CreateJurisdictionResponse ToViewModel(this HolidayJurisdiction jurisdiction) =>
        new CreateJurisdictionResponse(jurisdiction.Code, jurisdiction.Name);
}
