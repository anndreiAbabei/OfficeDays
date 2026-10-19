using OfficeDays.Features.Attendance.RecordToday.Contracts;

namespace OfficeDays.UnitTests.Features.Attendance.RecordToday.Contracts;

public sealed class RecordTodayRequestTests
{
    [Fact]
    public void Should_HaveNullBody()
    {
        // arrange
        // act
        var sut = new RecordTodayRequest();

        // assert
        Assert.Null(sut.Body);
    }
    
    [Fact]
    public void Should_SetBody()
    {
        // arrange
        var body = new RecordTodayRequestBody(true);
        
        // act
        var sut = new RecordTodayRequest
        {
            Body = body
        };

        // assert
        Assert.Same(body, sut.Body);
    }
}
