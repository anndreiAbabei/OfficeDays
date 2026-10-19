using OfficeDays.Features.Status.GetStatus;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Status.GetStatus;

public sealed class GetStatusMappingTests
{
    [Fact]
    public void Should_MapToViewModel()
    {
        // arrange
        var sut = new AttendanceStatus("2026-10", 22, 10, 12, 8, 4, 66.67m);

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Equal(sut.Period, result.Period);
        Assert.Equal(sut.EligibleWorkingDays, result.EligibleWorkingDays);
        Assert.Equal(sut.MaximumWfhDays, result.MaximumWfhDays);
        Assert.Equal(sut.RequiredOfficeDays, result.RequiredOfficeDays);
        Assert.Equal(sut.OfficeDays, result.OfficeDays);
        Assert.Equal(sut.RemainingOfficeDays, result.RemainingOfficeDays);
        Assert.Equal(sut.ProgressPercentage, result.ProgressPercentage);
        Assert.Equal(0, result.UpcomingPlannedDays);
    }
}
