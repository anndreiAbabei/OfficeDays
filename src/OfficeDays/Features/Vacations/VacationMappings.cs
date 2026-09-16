using OfficeDays.Domain;

namespace OfficeDays.Features.Vacations;

public static class VacationMappings
{
    public static VacationResponse ToViewModel(this Vacation vacation) => new VacationResponse(vacation.Id, vacation.From, vacation.To, vacation.CreatedAt);
}
