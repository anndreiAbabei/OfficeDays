using OfficeDays.Features.Attendance.RecordToday.Contracts;

namespace OfficeDays.UnitTests.Features.Attendance.RecordToday.Contracts;

public sealed class RecordTodayResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);
        var expectedCreated = DateTimeOffset.Now;
        const bool expectedIsManual = true;
        
        // act
        var sut = new RecordTodayResponse(expectedDate, expectedCreated, expectedIsManual);

        // assert
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedCreated, sut.CreatedAt);
        Assert.Equal(expectedIsManual, sut.IsManual);
    }
}
