using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;

public sealed class UpdateJurisdictionResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedCode = "RO";
        var expectedName = "Name value";

        // act
        var sut = new UpdateJurisdictionResponse(expectedCode, expectedName);

        // assert
        Assert.Equal(expectedCode, sut.Code);
        Assert.Equal(expectedName, sut.Name);
    }
}
