using OfficeDays.Features.Vacations.CreateVacation.Contracts;

namespace OfficeDays.UnitTests.Features.Vacations.CreateVacation.Contracts;

public sealed class CreateVacationResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedFrom = new DateOnly(2026, 10, 2);
        var expectedTo = new DateOnly(2026, 10, 3);
        var expectedCreatedAt = new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero);

        // act
        var sut = new CreateVacationResponse(expectedId, expectedFrom, expectedTo, expectedCreatedAt);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedFrom, sut.From);
        Assert.Equal(expectedTo, sut.To);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
    }
}
