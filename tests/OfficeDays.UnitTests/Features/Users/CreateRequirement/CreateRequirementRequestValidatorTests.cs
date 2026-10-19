using OfficeDays.Features.Users.CreateRequirement.Contracts;
using OfficeDays.Features.Users.CreateRequirement;

namespace OfficeDays.UnitTests.Features.Users.CreateRequirement;

public sealed class CreateRequirementRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new CreateRequirementRequestValidator(new CreateRequirementRequestBodyValidator());
        var request = new CreateRequirementRequest
        {
            Body = new CreateRequirementRequestBody
            {
                StartingFrom = new DateOnly(2026, 6, 1), 
                RequiredOfficePercentage = 50
            }
        };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var sut = new CreateRequirementRequestValidator(new CreateRequirementRequestBodyValidator());
        var request = new CreateRequirementRequest { Body = null! };

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
        var sut = new CreateRequirementRequestValidator(new CreateRequirementRequestBodyValidator());
        var request = new CreateRequirementRequest { Body = new CreateRequirementRequestBody { StartingFrom = DateOnly.FromDateTime(DateTime.Now), RequiredOfficePercentage = 50 } with { RequiredOfficePercentage = -1 } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.RequiredOfficePercentage");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCreatingBaseline()
    {
        // arrange
        var sut = new CreateRequirementRequestValidator(new CreateRequirementRequestBodyValidator());
        var request = new CreateRequirementRequest { Body = new CreateRequirementRequestBody { StartingFrom = DateOnly.FromDateTime(DateTime.Now), RequiredOfficePercentage = 50 } with { StartingFrom = DateOnly.MinValue } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.StartingFrom");
    }
}
