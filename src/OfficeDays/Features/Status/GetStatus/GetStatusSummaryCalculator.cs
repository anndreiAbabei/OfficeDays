using OfficeDays.Domain;
using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Services;

namespace OfficeDays.Features.Status.GetStatus;

public static class GetStatusSummaryCalculator
{
    public static GetStatusSummary Calculate(IReadOnlyList<PlanningDay> days,
                                            DateOnly today,
                                            AttendanceStatus status,
                                            UserAttendanceRequirement requirement)
    {
        var weekdays = days.Where(day => day.Date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                           .ToArray();
        var holidayWorkingDays = weekdays.Count(day => day.Holidays.Count > 0);
        // A vacation on a bank holiday must not deduct the same weekday twice.
        var vacationWorkingDays = weekdays.Count(day => day.IsVacation && day.Holidays.Count == 0);
        var normallyRequired = (int)Math.Ceiling(weekdays.Length * requirement.EffectivePercentage / 100m);
        var remainingEligibleDays = days.Count(day => day.Date >= today && day.IsEligible && !day.IsAttended);

        return new GetStatusSummary(today,
                                    days.Count,
                                    weekdays.Length,
                                    holidayWorkingDays,
                                    vacationWorkingDays,
                                    requirement.RequirementMode,
                                    requirement.RequirementValue,
                                    normallyRequired,
                                    days.Count(day => day.IsPlanned && day.IsEligible),
                                    remainingEligibleDays,
                                    Math.Max(0, status.RemainingOfficeDays - remainingEligibleDays),
                                    days.SelectMany(day => day.Holidays.Select(name => new GetStatusHoliday(day.Date, name))).ToArray(),
                                    days.Where(day => day.IsVacation).Select(day => day.Date).ToArray());
    }
}
