namespace OfficeDays.Services;

public sealed record PlanningDay(DateOnly Date, bool IsPlanned, bool IsEligible, bool IsAttended,
    bool IsToday, bool CanToggle, string Status, IReadOnlyList<string> Holidays, bool IsVacation);

public static class PlanningCalculator
{
    public static IReadOnlyList<PlanningDay> Calculate(CalculationPeriod period, DateOnly today,
        IEnumerable<DateOnly> plans, IEnumerable<DateOnly> attendance,
        IEnumerable<(DateOnly Date, string Name)> holidays,
        IEnumerable<(DateOnly From, DateOnly To)> vacations)
    {
        var planned = plans.ToHashSet();
        var attended = attendance.ToHashSet();
        var holidayLookup = holidays.ToLookup(x => x.Date, x => x.Name);
        var ranges = vacations.ToArray();
        var days = new List<PlanningDay>();
        for (var number = period.Start.DayNumber; number <= period.End.DayNumber; number++)
        {
            var date = DateOnly.FromDayNumber(number);
            var names = holidayLookup[date].ToArray();
            var vacation = ranges.Any(x => x.From <= date && date <= x.To);
            var eligible = date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday
                && names.Length == 0 && !vacation;
            var isPlanned = planned.Contains(date);
            var isAttended = attended.Contains(date);
            var status = isAttended ? "attended" : vacation || names.Length > 0 ? "holiday"
                : isPlanned ? date < today ? "missed" : "planned" : "none";
            days.Add(new PlanningDay(date, isPlanned, eligible, isAttended, date == today,
                                     date >= today && eligible, status, names, vacation));
        }
        return days;
    }

    public static int UpcomingCount(IEnumerable<PlanningDay> days, DateOnly today) =>
        days.Count(x => x.Date >= today && x.IsPlanned && x.IsEligible && !x.IsAttended);
}
