using OfficeDays.Features.Vacations.GetVacations.Contracts;
using OfficeDays.Features.Vacations.GetVacations;

namespace OfficeDays.UnitTests.Features.Vacations.GetVacations;

public sealed class GetVacationsRequestValidatorTests
{
    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(9999, 12, true)]
    [InlineData(2026, 6, true)]
    [InlineData(0, 6, false)]
    [InlineData(10000, 6, false)]
    [InlineData(2026, 0, false)]
    [InlineData(2026, 13, false)]
    [InlineData(null, null, true)]
    [InlineData(2026, null, false)]
    [InlineData(null, 6, false)]
    public void Should_ValidateYearAndMonth(int? year, int? month, bool expectedIsValid)
    {
        // arrange
        var request = new GetVacationsRequest { Year = year, Month = month };
        var sut = new GetVacationsRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.Equal(expectedIsValid, result.IsValid);
    }
}
