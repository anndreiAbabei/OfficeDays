using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Vacations.GetVacations.Contracts;

public sealed record GetVacationsItem(Guid Id, DateOnly From, DateOnly To, DateTimeOffset CreatedAt);

public sealed record GetVacationsResponse : CollectionResponse<GetVacationsItem>
{
    public GetVacationsResponse(IEnumerable<GetVacationsItem> items) : base(items) {}
}
