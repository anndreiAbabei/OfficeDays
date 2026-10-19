using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;

public sealed class DeleteJurisdictionRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedCode = "RO";

        // act
        var sut = new DeleteJurisdictionRequest
        {
            Code = expectedCode
        };

        // assert
        Assert.Equal(expectedCode, sut.Code);
    }
}
