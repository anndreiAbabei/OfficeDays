using OfficeDays.Features.Vacations.UpdateVacation.Contracts;
using OfficeDays.Features.Vacations.UpdateVacation;

namespace OfficeDays.UnitTests.Features.Vacations.UpdateVacation;

public sealed class UpdateVacationRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var request = new UpdateVacationRequestBody(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12));
        var sut = new UpdateVacationRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenToIsBeforeStart()
    {
        // arrange
        var request = new UpdateVacationRequestBody(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12)) with { To = new DateOnly(2026, 10, 9) };
        var sut = new UpdateVacationRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "To");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenToIsSameAsStart()
    {
        // arrange
        var request = new UpdateVacationRequestBody(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12)) with { To = new DateOnly(2026, 10, 10) };
        var sut = new UpdateVacationRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
