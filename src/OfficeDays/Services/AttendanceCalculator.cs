namespace OfficeDays.Services;

public readonly record struct CalculationPeriod(DateOnly Start, DateOnly End, string Label)
{
    public static CalculationPeriod ForMonth(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        
        return new CalculationPeriod(start, start.AddMonths(1).AddDays(-1), $"{year:D4}-{month:D2}");
    }
}

public sealed record AttendanceStatus(string Period,
                                      int EligibleWorkingDays,
                                      int MaximumWfhDays,
                                      int RequiredOfficeDays,
                                      int OfficeDays,
                                      int RemainingOfficeDays,
                                      decimal ProgressPercentage);

public static class AttendanceCalculator
{
    public static AttendanceStatus Calculate(CalculationPeriod period,
                                             IEnumerable<DateOnly> bankHolidays,
                                             IEnumerable<(DateOnly From, DateOnly To)> vacations,
                                             IEnumerable<DateOnly> officeDates)
    {
        if (period.End < period.Start)
        {
            throw new ArgumentException("The calculation period end cannot precede its start.", nameof(period));
        }

        var holidays = bankHolidays.ToHashSet();
        var vacationDates = ExpandVacations(vacations, period);
        var eligibleDates = EnumerateDates(period.Start, period.End).Where(IsWeekday)
                                                                    .Where(date => !holidays.Contains(date) && 
                                                                                   !vacationDates.Contains(date))
                                                                    .ToHashSet();

        var eligibleWorkingDays = eligibleDates.Count;
        var maximumWfhDays = (int)Math.Floor(eligibleWorkingDays * 0.5m);
        var requiredOfficeDays = eligibleWorkingDays - maximumWfhDays;
        var officeDays = officeDates.Distinct().Count(eligibleDates.Contains);
        var remainingOfficeDays = Math.Max(0, requiredOfficeDays - officeDays);
        var progress = requiredOfficeDays == 0
            ? 100m
            : Math.Min(100m, Math.Round(officeDays * 100m / requiredOfficeDays, 1));

        return new AttendanceStatus(period.Label, eligibleWorkingDays, maximumWfhDays, requiredOfficeDays,
                                    officeDays, remainingOfficeDays, progress);
    }

    private static HashSet<DateOnly> ExpandVacations(IEnumerable<(DateOnly From, DateOnly To)> vacations,
                                                     CalculationPeriod period)
    {
        var dates = new HashSet<DateOnly>();
        foreach (var (from, to) in vacations)
        {
            if (to < from)
            {
                throw new ArgumentException("A vacation end date cannot precede its start date.", nameof(vacations));
            }

            var start = from < period.Start ? period.Start : from;
            var end = to > period.End ? period.End : to;
            
            if (start > end)
                continue;
            
            foreach (var date in EnumerateDates(start, end))
                dates.Add(date);
        }

        return dates;
    }

    private static IEnumerable<DateOnly> EnumerateDates(DateOnly start, DateOnly end)
    {
        for (var date = start; date <= end; date = date.AddDays(1))
            yield return date;
    }

    private static bool IsWeekday(DateOnly date) => date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
}
