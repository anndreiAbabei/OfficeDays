using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Features.Status.GetStatus;

namespace OfficeDays.UnitTests.Features.Status.GetStatus;

public sealed class GetStatusRequestValidatorTests
{
    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(9998, 12, true)]
    [InlineData(2026, 6, true)]
    [InlineData(0, 6, false)]
    [InlineData(9999, 6, false)]
    [InlineData(2026, 0, false)]
    [InlineData(2026, 13, false)]
    [InlineData(null, null, true)]
    [InlineData(2026, null, false)]
    [InlineData(null, 6, false)]
    public void Should_ValidateYearAndMonth(int? year, int? month, bool expectedIsValid)
    {
        // arrange
        var request = new GetStatusRequest { Year = year, Month = month };
        var sut = new GetStatusRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.Equal(expectedIsValid, result.IsValid);
    }
}
