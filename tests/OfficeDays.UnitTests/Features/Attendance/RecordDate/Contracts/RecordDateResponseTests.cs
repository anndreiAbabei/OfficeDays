using OfficeDays.Features.Attendance.RecordDate.Contracts;

namespace OfficeDays.UnitTests.Features.Attendance.RecordDate.Contracts;

public sealed class RecordDateResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedDate = new DateOnly(2020, 01, 13);
        var expectedCreatedTime = DateTime.Now;
        const bool expectedIsManual = true;
        
        // act
        var sut = new RecordDateResponse(expectedDate, expectedCreatedTime, expectedIsManual);

        // assert
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedCreatedTime, sut.CreatedAt);
        Assert.Equal(expectedIsManual, sut.IsManual);
    }
}
