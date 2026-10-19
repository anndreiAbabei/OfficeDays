using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class PersonalDataExportTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const string expectedFormat = "officedays-personal-data";
        const int expectedFormatVersion = 1;
        var expectedExportedAt = new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero);
        var expectedApplication = new ExportApplication("OfficeDays", "2.2.1+test");
        var expectedReportContext = new ExportReportContext(1, new DateOnly(2026, 10, 19), new DateOnly(2026, 1, 1));
        var expectedProfile = new ExportProfile(Guid.NewGuid(), "User", "USER", "user@example.com", "Europe/Bucharest", 42, true, new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero));
        var expectedPreferences = new ExportPreferences("thursday");
        var expectedRequirementHistory = new[] { new ExportRequirement(Guid.NewGuid(), Guid.NewGuid(), DateOnly.MinValue, 75, new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero)) };
        var expectedAttendance = new[] { new ExportAttendance(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 10, 19), true, new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero)) };
        var expectedPlans = new[] { new ExportPlan(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 10, 19), new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero)) };
        var expectedVacations = new[] { new ExportVacation(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 10, 19), new DateOnly(2026, 10, 23), new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero)) };
        var expectedHolidayCalendar = new ExportHolidayCalendar(42, "RO", "Romania", new[] { new ExportHoliday(Guid.NewGuid(), 42, new DateOnly(2026, 10, 19), "Holiday") });

        // act
        var sut = new PersonalDataExport(expectedFormat, expectedFormatVersion, expectedExportedAt, expectedApplication, expectedReportContext, expectedProfile, expectedPreferences, expectedRequirementHistory, expectedAttendance, expectedPlans, expectedVacations, expectedHolidayCalendar);

        // assert
        Assert.Equal(expectedFormat, sut.Format);
        Assert.Equal(expectedFormatVersion, sut.FormatVersion);
        Assert.Equal(expectedExportedAt, sut.ExportedAt);
        Assert.Equal(expectedApplication, sut.Application);
        Assert.Equal(expectedReportContext, sut.ReportContext);
        Assert.Equal(expectedProfile, sut.Profile);
        Assert.Equal(expectedPreferences, sut.Preferences);
        Assert.Equal(expectedRequirementHistory, sut.RequirementHistory);
        Assert.Equal(expectedAttendance, sut.Attendance);
        Assert.Equal(expectedPlans, sut.Plans);
        Assert.Equal(expectedVacations, sut.Vacations);
        Assert.Equal(expectedHolidayCalendar, sut.HolidayCalendar);
    }
}
