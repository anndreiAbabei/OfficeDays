using OfficeDays.Features.Attendance.GetAttendance.Contracts;

namespace OfficeDays.UnitTests.Features.Attendance.Contracts;

public sealed class GetAttendanceRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const int expectedYear = 2021;
        const int expectedMonth = 12;
        
        // act
        var sut = new GetAttendanceRequest
        {
            Year = expectedYear,
            Month = expectedMonth
        };

        // assert
        Assert.Equal(expectedYear, sut.Year);
        Assert.Equal(expectedMonth, sut.Month);
    }
}
