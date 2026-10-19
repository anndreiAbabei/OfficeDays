using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;

public sealed class CreateJurisdictionResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const string expectedCode = "RO";
        const string expectedName = "Name value";

        // act
        var sut = new CreateJurisdictionResponse(expectedCode, expectedName);

        // assert
        Assert.Equal(expectedCode, sut.Code);
        Assert.Equal(expectedName, sut.Name);
    }
}
