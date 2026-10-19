using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;

public sealed class CreateJurisdictionRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedBody = new CreateJurisdictionRequestBody("RO", "Name value");

        // act
        var sut = new CreateJurisdictionRequest
        {
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedBody, sut.Body);
    }
}
