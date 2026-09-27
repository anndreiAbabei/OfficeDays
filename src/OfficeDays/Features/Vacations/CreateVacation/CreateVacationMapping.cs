using OfficeDays.Features.Vacations.CreateVacation.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Vacations.CreateVacation;

public static class CreateVacationMapping
{
    public static CreateVacationResponse ToViewModel(this Vacation vacation) => new CreateVacationResponse(vacation.Id, vacation.From, vacation.To, vacation.CreatedAt);
}
