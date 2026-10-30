using System.Text.Json;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Services;

public sealed class GuestCalculationParityTests
{
    [Fact]
    public void Should_MatchGuestScenarios_WhenCalculatingAttendance()
    {
        // Arrange
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "attendance-calculations.json")));
        var calculator = new AttendanceCalculator();

        foreach (var scenario in document.RootElement.EnumerateArray())
        {
            var period = CalculationPeriod.ForMonth(scenario.GetProperty("year").GetInt32(), scenario.GetProperty("month").GetInt32());
            var holidays = scenario.GetProperty("holidays").EnumerateArray().Select(x => ParseDate(x)).ToArray();
            var vacations = scenario.GetProperty("vacations").EnumerateArray()
                .Select(x => (ParseDate(x.GetProperty("from")), ParseDate(x.GetProperty("to")))).ToArray();
            var attendance = scenario.GetProperty("attendance").EnumerateArray().Select(x => ParseDate(x)).ToArray();
            var percentage = scenario.GetProperty("value").GetInt32() * (scenario.GetProperty("mode").GetString() == "DaysPerWeek" ? 20 : 1);
            var expectedEligible = scenario.GetProperty("eligible").GetInt32();
            var expectedRequired = scenario.GetProperty("required").GetInt32();
            var expectedRecorded = scenario.GetProperty("recorded").GetInt32();
            var expectedProgress = scenario.GetProperty("progress").GetDecimal();

            // Act
            var actual = calculator.Calculate(period, holidays, vacations, attendance, percentage);

            // Assert
            Assert.Equal(expectedEligible, actual.EligibleWorkingDays);
            Assert.Equal(expectedRequired, actual.RequiredOfficeDays);
            Assert.Equal(expectedRecorded, actual.OfficeDays);
            Assert.Equal(expectedProgress, actual.ProgressPercentage);
        }
    }

    private static DateOnly ParseDate(JsonElement value)
    {
        var text = value.GetString() ?? throw new InvalidDataException("Fixture date is missing.");
        return DateOnly.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
    }
}
