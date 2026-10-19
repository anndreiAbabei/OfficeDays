using OfficeDays.Features.Attendance.RecordDate.Contracts;

namespace OfficeDays.UnitTests.Features.Attendance.RecordDate.Contracts;

public sealed class RecordDateRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedDate = new DateOnly(2021, 10, 22);
        
        // act
        var sut = new RecordDateRequest
        {
            Date = expectedDate
        };

        // assert
        Assert.Equal(expectedDate, sut.Date);
    }
}
