using OfficeDays.Domain;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;
using OfficeDays.Features.Users.UpdateRequirement;

namespace OfficeDays.UnitTests.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new UpdateRequirementRequestValidator(new UpdateRequirementRequestBodyValidator());
        var request = new UpdateRequirementRequest
        {
            Body = new UpdateRequirementRequestBody
            {
                RequirementMode = AttendanceRequirementMode.Percentage,
                StartingFrom = new DateOnly(2026, 10, 01), 
                RequirementValue = 50
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
        var sut = new UpdateRequirementRequestValidator(new UpdateRequirementRequestBodyValidator());
        var request = new UpdateRequirementRequest { Body = null! };

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
        var sut = new UpdateRequirementRequestValidator(new UpdateRequirementRequestBodyValidator());
        var request = new UpdateRequirementRequest { Body = new UpdateRequirementRequestBody { RequirementMode = AttendanceRequirementMode.Percentage, StartingFrom = DateOnly.FromDateTime(DateTime.Now), RequirementValue = 50 } with { RequirementValue = -1 } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.RequirementValue");
    }
}
