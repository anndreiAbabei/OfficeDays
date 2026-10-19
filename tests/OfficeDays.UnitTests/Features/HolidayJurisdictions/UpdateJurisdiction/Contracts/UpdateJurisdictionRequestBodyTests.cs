using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;

public sealed class UpdateJurisdictionRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedName = "Name value";

        // act
        var sut = new UpdateJurisdictionRequestBody(expectedName);

        // assert
        Assert.Equal(expectedName, sut.Name);
    }
}
