using OfficeDays.Domain;
using OfficeDays.Features.Vacations.GetVacations;

namespace OfficeDays.UnitTests.Features.Vacations.GetVacations;

public sealed class GetVacationsMappingTests
{
    [Fact]
    public void Should_MapToViewModel()
    {
        // arrange
        var sut = new Vacation { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), From = new DateOnly(2026, 10, 3), To = new DateOnly(2026, 10, 8), CreatedAt = new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero) };

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Equal(sut.Id, result.Id);
        Assert.Equal(sut.From, result.From);
        Assert.Equal(sut.To, result.To);
        Assert.Equal(sut.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public void Should_MapCollectionInSourceOrder()
    {
        // arrange
        var firstEntity = new Vacation { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), From = new DateOnly(2026, 10, 3), To = new DateOnly(2026, 10, 8), CreatedAt = new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero) };
        var secondEntity = new Vacation { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), From = new DateOnly(2026, 12, 5), To = new DateOnly(2026, 10, 8), CreatedAt = new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero) };
        Vacation[] sut = [firstEntity, secondEntity];

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Collection(result,
            first =>
            {
                Assert.Equal(firstEntity.Id, first.Id);
                Assert.Equal(firstEntity.From, first.From);
                Assert.Equal(firstEntity.To, first.To);
                Assert.Equal(firstEntity.CreatedAt, first.CreatedAt);
            },
            second =>
            {
                Assert.Equal(secondEntity.Id, second.Id);
                Assert.Equal(secondEntity.From, second.From);
                Assert.Equal(secondEntity.To, second.To);
                Assert.Equal(secondEntity.CreatedAt, second.CreatedAt);
            });
    }

    [Fact]
    public void Should_MapEmptyCollection()
    {
        // arrange
        Vacation[] sut = [];

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Empty(result);
    }
}
