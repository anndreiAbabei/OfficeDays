using OfficeDays.Features.Vacations.UpdateVacation.Contracts;
using OfficeDays.Features.Vacations.UpdateVacation;

namespace OfficeDays.UnitTests.Features.Vacations.UpdateVacation;

public sealed class UpdateVacationRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new UpdateVacationRequestValidator(new UpdateVacationRequestBodyValidator());
        var request = new UpdateVacationRequest { Body = new UpdateVacationRequestBody(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12)) };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var sut = new UpdateVacationRequestValidator(new UpdateVacationRequestBodyValidator());
        var request = new UpdateVacationRequest { Body = null! };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsInvalid()
    {
        // arrange
        var sut = new UpdateVacationRequestValidator(new UpdateVacationRequestBodyValidator());
        var request = new UpdateVacationRequest { Body = new UpdateVacationRequestBody(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12)) with { To = new DateOnly(2026, 10, 9) } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.To");
    }
}
