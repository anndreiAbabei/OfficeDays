using OfficeDays.Features.Vacations.UpdateVacation.Contracts;

namespace OfficeDays.UnitTests.Features.Vacations.UpdateVacation.Contracts;

public sealed class UpdateVacationRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedFrom = DateOnly.FromDateTime(DateTime.Now);
        var expectedTo = new DateOnly(2026, 10, 2);

        // act
        var sut = new UpdateVacationRequestBody(expectedFrom, expectedTo);

        // assert
        Assert.Equal(expectedFrom, sut.From);
        Assert.Equal(expectedTo, sut.To);
    }
}
