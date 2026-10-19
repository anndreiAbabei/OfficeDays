using OfficeDays.Services;
using OfficeDays.Domain;

namespace OfficeDays.UnitTests.Features.Planning;

public sealed class PlanningCalculatorTests
{
    private static DateOnly D(int day) => new DateOnly(2026, 10, day);

    [Fact]
    public void Calendar_resolves_overlaps_and_counts_only_remaining_eligible_plans()
    {
        var days = PlanningCalculator.Calculate(CalculationPeriod.ForMonth(2026, 10), D(10),
            [D(6), D(7), D(10), D(12), D(13), D(14), D(15)], [D(7), D(8), D(15)],
            [(D(15), "Holiday")], [(D(14), D(15))]);
        Assert.Equal("missed", days.Single(x => x.Date == D(6)).Status);
        Assert.Equal("attended", days.Single(x => x.Date == D(7)).Status);
        Assert.Equal("attended", days.Single(x => x.Date == D(8)).Status);
        Assert.True(days.Single(x => x.Date == D(10)).IsToday);
        Assert.False(days.Single(x => x.Date == D(10)).CanToggle); // Saturday
        Assert.Equal("planned", days.Single(x => x.Date == D(12)).Status);
        Assert.Equal("holiday", days.Single(x => x.Date == D(14)).Status);
        var overlap = days.Single(x => x.Date == D(15));
        Assert.Equal("attended", overlap.Status);
        Assert.True(overlap.IsVacation);
        Assert.Equal("Holiday", Assert.Single(overlap.Holidays));
        Assert.Equal(2, PlanningCalculator.UpcomingCount(days, D(10)));
        Assert.All(days.Where(x => x.Date < D(10)), x => Assert.False(x.CanToggle));
    }

    [Fact]
    public void Today_stays_planned_until_next_local_day_and_attendance_preserves_plan()
    {
        var period = CalculationPeriod.ForMonth(2026, 10);
        var today = PlanningCalculator.Calculate(period, D(12), [D(12)], [], [], []);
        Assert.Equal("planned", today[11].Status);
        Assert.True(today[11].CanToggle);
        Assert.Equal(1, PlanningCalculator.UpcomingCount(today, D(12)));
        var tomorrow = PlanningCalculator.Calculate(period, D(13), [D(12)], [], [], []);
        Assert.Equal("missed", tomorrow[11].Status);
        Assert.Equal(0, PlanningCalculator.UpcomingCount(tomorrow, D(13)));
        var attended = PlanningCalculator.Calculate(period, D(12), [D(12)], [D(12)], [], []);
        Assert.True(attended[11].IsPlanned);
        Assert.True(attended[11].IsToday);
        Assert.Equal("attended", attended[11].Status);
        Assert.Equal(0, PlanningCalculator.UpcomingCount(attended, D(12)));
    }

    [Theory]
    [InlineData(2028, 2, 29)]
    [InlineData(2026, 2, 28)]
    [InlineData(9999, 12, 31)]
    [InlineData(1, 1, 31)]
    public void Calendar_handles_month_boundaries(int year, int month, int count)
    {
        var days = PlanningCalculator.Calculate(CalculationPeriod.ForMonth(year, month), D(10), [], [], [], []);
        Assert.Equal(count, days.Count);
    }

    [Fact]
    public void User_timezone_decides_whether_a_plan_is_missed()
    {
        var clock = new Clock(new DateTimeOffset(2026, 10, 12, 22, 30, 0, TimeSpan.Zero));
        var dates = new UserDateService(clock);
        var user = new User { Username = "test", NormalizedUsername = "TEST", PasswordHash = "hash", TimeZoneId = "Europe/Bucharest" };
        var today = dates.Today(user);
        Assert.Equal(D(13), today);
        Assert.Equal("missed", PlanningCalculator.Calculate(new CalculationPeriod(D(12), D(12), ""), today, [D(12)], [], [], [])[0].Status);
    }

    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
