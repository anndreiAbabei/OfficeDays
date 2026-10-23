using OfficeDays.Features.Reports.GetReport.Contracts;

namespace OfficeDays.UnitTests.Features.Reports.GetReport.Contracts;

public sealed class GetReportMonthTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedMonth = 21;
        var expectedEligibleWorkingDays = 22;
        var expectedValue = 23;
        var expectedRequiredOfficeDays = 24;
        var expectedOfficeDays = 25;
        var expectedAttendancePercentage = 47.5m;
        var expectedResult = "Result value";

        // act
        var sut = new GetReportMonth(expectedMonth, expectedEligibleWorkingDays, expectedValue, expectedRequiredOfficeDays, expectedOfficeDays, expectedAttendancePercentage, expectedResult);

        // assert
        Assert.Equal(expectedMonth, sut.Month);
        Assert.Equal(expectedEligibleWorkingDays, sut.EligibleWorkingDays);
        Assert.Equal(expectedValue, sut.RequirementValue);
        Assert.Equal(expectedRequiredOfficeDays, sut.RequiredOfficeDays);
        Assert.Equal(expectedOfficeDays, sut.OfficeDays);
        Assert.Equal(expectedAttendancePercentage, sut.AttendancePercentage);
        Assert.Equal(expectedResult, sut.Result);
    }
}
