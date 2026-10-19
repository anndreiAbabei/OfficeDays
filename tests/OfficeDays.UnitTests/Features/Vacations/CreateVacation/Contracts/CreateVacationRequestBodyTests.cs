using OfficeDays.Features.Vacations.CreateVacation.Contracts;

namespace OfficeDays.UnitTests.Features.Vacations.CreateVacation.Contracts;

public sealed class CreateVacationRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedFrom = DateOnly.FromDateTime(DateTime.Now);
        var expectedTo = new DateOnly(2026, 10, 2);

        // act
        var sut = new CreateVacationRequestBody(expectedFrom, expectedTo);

        // assert
        Assert.Equal(expectedFrom, sut.From);
        Assert.Equal(expectedTo, sut.To);
    }
}
