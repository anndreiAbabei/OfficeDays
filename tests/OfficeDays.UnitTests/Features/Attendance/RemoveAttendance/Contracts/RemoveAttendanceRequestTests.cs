using OfficeDays.Features.Attendance.RemoveAttendance.Contracts;

namespace OfficeDays.UnitTests.Features.Attendance.RemoveAttendance.Contracts;

public sealed class RemoveAttendanceRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);
        
        // act
        var sut = new RemoveAttendanceRequest
        {
            Date = expectedDate
        };

        // assert
        Assert.Equal(expectedDate, sut.Date);
    }
}
