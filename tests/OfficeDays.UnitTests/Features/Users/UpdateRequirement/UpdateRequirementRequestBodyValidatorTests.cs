using OfficeDays.Features.Users.UpdateRequirement.Contracts;
using OfficeDays.Features.Users.UpdateRequirement;

namespace OfficeDays.UnitTests.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var request = new UpdateRequirementRequestBody
        {
            StartingFrom = new DateOnly(2026, 10, 1),
            RequiredOfficePercentage = 50
        };
        var sut = new UpdateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenRequiredOfficePercentageIsBelowZero()
    {
        // arrange
        var request = new UpdateRequirementRequestBody { StartingFrom = DateOnly.FromDateTime(DateTime.Now), RequiredOfficePercentage = 50 } with { RequiredOfficePercentage = -1 };
        var sut = new UpdateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "RequiredOfficePercentage");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenRequiredOfficePercentageIsAboveOneHundred()
    {
        // arrange
        var request = new UpdateRequirementRequestBody { StartingFrom = DateOnly.FromDateTime(DateTime.Now), RequiredOfficePercentage = 50 } with { RequiredOfficePercentage = 101 };
        var sut = new UpdateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "RequiredOfficePercentage");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenStartingFromIsNotFirstDayOfMonth()
    {
        // arrange
        var request = new UpdateRequirementRequestBody { StartingFrom = DateOnly.FromDateTime(DateTime.Now), RequiredOfficePercentage = 50 } with { StartingFrom = new DateOnly(2026, 10, 2) };
        var sut = new UpdateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "StartingFrom");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenRequiredOfficePercentageIsZero()
    {
        // arrange
        var request = new UpdateRequirementRequestBody
        {
            StartingFrom = new DateOnly(2026, 10, 1), 
            RequiredOfficePercentage = 0
        };
        var sut = new UpdateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenRequiredOfficePercentageIsOneHundred()
    {
        // arrange
        var request = new UpdateRequirementRequestBody
        {
            StartingFrom = new DateOnly(2026, 10, 1), 
            RequiredOfficePercentage = 100
        };
        var sut = new UpdateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenStartingFromIsBaselineDate()
    {
        // arrange
        var request = new UpdateRequirementRequestBody { StartingFrom = DateOnly.FromDateTime(DateTime.Now), RequiredOfficePercentage = 50 } with { StartingFrom = DateOnly.MinValue };
        var sut = new UpdateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
