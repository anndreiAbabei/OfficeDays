using OfficeDays.Features.Vacations.CreateVacation.Contracts;
using OfficeDays.Features.Vacations.CreateVacation;

namespace OfficeDays.UnitTests.Features.Vacations.CreateVacation;

public sealed class CreateVacationRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new CreateVacationRequestValidator(new CreateVacationRequestBodyValidator());
        var request = new CreateVacationRequest { Body = new CreateVacationRequestBody(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12)) };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var sut = new CreateVacationRequestValidator(new CreateVacationRequestBodyValidator());
        var request = new CreateVacationRequest { Body = null! };

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
        var sut = new CreateVacationRequestValidator(new CreateVacationRequestBodyValidator());
        var request = new CreateVacationRequest { Body = new CreateVacationRequestBody(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12)) with { To = new DateOnly(2026, 10, 9) } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.To");
    }
}
