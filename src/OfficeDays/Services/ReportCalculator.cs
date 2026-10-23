using OfficeDays.Domain;

namespace OfficeDays.Services;

public interface IReportCalculator
{
    AttendanceReport Calculate(int year, int startMonth, int endMonth, User user, DateTimeOffset generatedAt,
                               IEnumerable<UserAttendanceRequirement> requirements,
                               IEnumerable<DateOnly> bankHolidays,
                               IEnumerable<(DateOnly From, DateOnly To)> vacations, 
                               IEnumerable<DateOnly> officeDates);
}

public sealed class ReportCalculator(IAttendanceCalculator attendanceCalculator) : IReportCalculator
{
    private readonly IAttendanceCalculator _attendanceCalculator = attendanceCalculator;

    public AttendanceReport Calculate(int year, int startMonth, int endMonth, User user, DateTimeOffset generatedAt,
                                       IEnumerable<UserAttendanceRequirement> requirements,
                                       IEnumerable<DateOnly> bankHolidays,
                                       IEnumerable<(DateOnly From, DateOnly To)> vacations, 
                                       IEnumerable<DateOnly> officeDates)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(generatedAt, zone).DateTime);
        var registered = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(user.CreatedAt, zone).DateTime);
        var registrationMonth = new DateOnly(registered.Year, registered.Month, 1);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);
        var schedule = requirements.OrderByDescending(x => x.StartingFrom).ToArray();
        var holidays = bankHolidays.ToArray();
        var timeOff = vacations.ToArray();
        var attendance = officeDates.Where(x => x <= today).ToArray();
        var months = new List<AttendanceReportMonth>();

        for (var month = startMonth; month <= endMonth; month++)
        {
            var period = CalculationPeriod.ForMonth(year, month);
            if (period.Start < registrationMonth)
                continue;

            var requirement = schedule.First(x => x.StartingFrom <= period.Start);
            var percentage = requirement.EffectivePercentage;
            var status = _attendanceCalculator.Calculate(period, holidays, timeOff, attendance, percentage);
            var future = period.Start > currentMonth;
            var state = future ? ReportMonthState.NotStarted
                : period.Start == currentMonth ? ReportMonthState.InProgress
                : status.OfficeDays >= status.RequiredOfficeDays ? ReportMonthState.TargetMet
                : ReportMonthState.BelowTarget;

            months.Add(new AttendanceReportMonth(month, status.EligibleWorkingDays, requirement.RequirementValue, status.RequiredOfficeDays,
                                                 future ? null : status.OfficeDays, 
                                                 future ? null : Percentage(status.OfficeDays, status.EligibleWorkingDays), 
                                                 state)
            {
                RequirementMode = requirement.RequirementMode
            });
        }

        var started = months.Where(x => x.State != ReportMonthState.NotStarted).ToArray();
        var completed = months.Where(x => x.State is ReportMonthState.TargetMet or ReportMonthState.BelowTarget).ToArray();
        var officeDays = started.Sum(x => x.OfficeDays ?? 0);
        
        return new AttendanceReport(year, startMonth, endMonth, user.Username, user.TimeZoneId, registrationMonth, today, 
                                    generatedAt, months, officeDays, 
                                    Percentage(officeDays, started.Sum(x => x.EligibleWorkingDays)),
                                    completed.Length, 
                                    completed.Count(x => x.State == ReportMonthState.TargetMet));
    }

    private static decimal? Percentage(int officeDays, int eligibleDays) => eligibleDays == 0 ? null : Math.Round(officeDays * 100m / eligibleDays, 1);
}
