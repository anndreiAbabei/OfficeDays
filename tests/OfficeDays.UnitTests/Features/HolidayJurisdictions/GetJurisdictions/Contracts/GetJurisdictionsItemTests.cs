using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.GetJurisdictions.Contracts;

public sealed class GetJurisdictionsItemTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedCode = "RO";
        var expectedName = "Name value";

        // act
        var sut = new GetJurisdictionsItem(expectedCode, expectedName);

        // assert
        Assert.Equal(expectedCode, sut.Code);
        Assert.Equal(expectedName, sut.Name);
    }
}
