using OfficeDays.Domain;

namespace OfficeDays.Features.Status.GetStatus.Contracts;

public sealed record GetStatusSummary(
    DateOnly Today,
    int TotalDays,
    int WorkingDays,
    int BankHolidayWorkingDays,
    int VacationWorkingDays,
    AttendanceRequirementMode RequirementMode,
    int RequirementValue,
    int RequiredOfficeDaysWithoutAbsences,
    int PlannedDays,
    int RemainingEligibleDays,
    int OutstandingOfficeDays,
    IReadOnlyList<GetStatusHoliday> BankHolidays,
    IReadOnlyList<DateOnly> VacationDates);
