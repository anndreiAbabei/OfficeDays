using OfficeDays.Domain;
using OfficeDays.Features.Vacations.CreateVacation;

namespace OfficeDays.UnitTests.Features.Vacations.CreateVacation;

public sealed class CreateVacationMappingTests
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
}
