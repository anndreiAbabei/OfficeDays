using OfficeDays.Domain;
using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.UpdateJurisdiction;

public sealed class UpdateJurisdictionMappingTests
{
    [Fact]
    public void Should_MapToViewModel()
    {
        // arrange
        var sut = new HolidayJurisdiction { Id = 7, Code = "RO", Name = "Romania" };

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Equal(sut.Code, result.Code);
        Assert.Equal(sut.Name, result.Name);
    }
}
