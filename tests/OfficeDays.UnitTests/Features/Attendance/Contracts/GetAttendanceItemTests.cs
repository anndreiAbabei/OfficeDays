using OfficeDays.Features.Attendance.GetAttendance.Contracts;

namespace OfficeDays.UnitTests.Features.Attendance.Contracts;

public sealed class GetAttendanceItemTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);
        var expectedCreatedAt = DateTimeOffset.Now;
        const bool expectedIsManual = true;
        
        // act
        var sut = new GetAttendanceItem(expectedDate, expectedCreatedAt, expectedIsManual);

        // assert
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
        Assert.Equal(expectedIsManual, sut.IsManual);
    }
}
