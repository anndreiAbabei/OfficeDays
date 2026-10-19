using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;

public sealed class UpdateJurisdictionRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedCode = "RO";
        var expectedBody = new UpdateJurisdictionRequestBody("Name value");

        // act
        var sut = new UpdateJurisdictionRequest
        {
            Code = expectedCode,
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedCode, sut.Code);
        Assert.Equal(expectedBody, sut.Body);
    }
}
