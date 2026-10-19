using OfficeDays.Services;

namespace OfficeDays.UnitTests.Services;

public sealed class AttendanceStatusTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const string expectedPeriod = "";
        const int expectedEligibleWorkingDays = 0;
        const int expectedMaximumWfhDays = 0;
        const int expectedRequiredOfficeDays = 0;
        const int expectedOfficeDays = 0;
        const int expectedRemainingOfficeDays = 0;
        const decimal expectedProgressPercentage = 0m;
        
        // act
        var sut = new AttendanceStatus
        (
             Period: expectedPeriod,
             EligibleWorkingDays: expectedEligibleWorkingDays,
             MaximumWfhDays: expectedMaximumWfhDays,
             RequiredOfficeDays: expectedRequiredOfficeDays,
             OfficeDays: expectedOfficeDays,
             RemainingOfficeDays: expectedRemainingOfficeDays,
             ProgressPercentage: expectedProgressPercentage
        );

        // assert
        Assert.Equal(expectedPeriod, sut.Period);
        Assert.Equal(expectedEligibleWorkingDays, sut.EligibleWorkingDays);
        Assert.Equal(expectedMaximumWfhDays, sut.MaximumWfhDays);
        Assert.Equal(expectedRequiredOfficeDays, sut.RequiredOfficeDays);
        Assert.Equal(expectedOfficeDays, sut.OfficeDays);
        Assert.Equal(expectedRemainingOfficeDays, sut.RemainingOfficeDays);
        Assert.Equal(expectedProgressPercentage, sut.ProgressPercentage);
    }
}
