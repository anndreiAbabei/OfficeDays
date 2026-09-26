using OfficeDays.Features.Vacations.GetVacations.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Vacations.GetVacations;

public static class GetVacationsMapping
{
    public static GetVacationsResponse ToViewModel(this Vacation vacation) => new GetVacationsResponse(vacation.Id, vacation.From, vacation.To, vacation.CreatedAt);
}
