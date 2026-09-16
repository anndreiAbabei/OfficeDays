namespace OfficeDays.Features.Vacations;

internal static class VacationValidation
{
    public static string? Validate(CreateVacationRequest request) =>
        request.To < request.From ? "Vacation end date cannot precede its start date." : null;
}
