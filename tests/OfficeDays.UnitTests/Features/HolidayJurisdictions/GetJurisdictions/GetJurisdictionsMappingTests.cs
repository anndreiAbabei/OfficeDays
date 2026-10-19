using OfficeDays.Domain;
using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.GetJurisdictions;

public sealed class GetJurisdictionsMappingTests
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

    [Fact]
    public void Should_MapCollectionInSourceOrder()
    {
        // arrange
        var firstEntity = new HolidayJurisdiction { Id = 7, Code = "RO", Name = "Romania" };
        var secondEntity = new HolidayJurisdiction { Id = 7, Code = "GB-NIR", Name = "Northern Ireland" };
        HolidayJurisdiction[] sut = [firstEntity, secondEntity];

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Collection(result,
            first =>
            {
                Assert.Equal(firstEntity.Code, first.Code);
                Assert.Equal(firstEntity.Name, first.Name);
            },
            second =>
            {
                Assert.Equal(secondEntity.Code, second.Code);
                Assert.Equal(secondEntity.Name, second.Name);
            });
    }

    [Fact]
    public void Should_MapEmptyCollection()
    {
        // arrange
        HolidayJurisdiction[] sut = [];

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Empty(result);
    }
}
