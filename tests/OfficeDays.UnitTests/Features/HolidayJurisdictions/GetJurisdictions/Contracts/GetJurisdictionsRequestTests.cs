using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.GetJurisdictions.Contracts;

public sealed class GetJurisdictionsRequestTests
{
    [Fact]
    public void Should_CreateRequest()
    {
        // arrange
        // No input is required.

        // act
        var sut = new GetJurisdictionsRequest();

        // assert
        Assert.IsAssignableFrom<IRequest>(sut);
    }
}
