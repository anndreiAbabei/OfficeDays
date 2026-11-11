using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;

public sealed record GetJurisdictionsItem(string Code, string Name);

public sealed record GetJurisdictionsResponse : CollectionResponse<GetJurisdictionsItem>
{
    public GetJurisdictionsResponse(IEnumerable<GetJurisdictionsItem> items) : base(items) {}
}
