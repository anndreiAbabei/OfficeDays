using OfficeDays.Features.Vacations.GetVacations.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Vacations.GetVacations;

public static class GetVacationsMapping
{
    public static GetVacationsResponse ToViewModel(this IEnumerable<Vacation> vacations)
    {
        return new GetVacationsResponse(vacations.Select(ToViewModel));
    }

    public static GetVacationsItem ToViewModel(this Vacation vacation)
    {
        return new GetVacationsItem(vacation.Id, vacation.From, vacation.To, vacation.CreatedAt);
    }
}
