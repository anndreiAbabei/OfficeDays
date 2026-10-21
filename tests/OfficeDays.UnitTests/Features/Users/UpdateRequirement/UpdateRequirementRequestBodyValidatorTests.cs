using OfficeDays.Domain;
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

    [Theory]
    [InlineData(AttendanceRequirementMode.Percentage, 0, true)]
    [InlineData(AttendanceRequirementMode.Percentage, 100, true)]
    [InlineData(AttendanceRequirementMode.Percentage, 101, false)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 0, true)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 3, true)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 5, true)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 6, false)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, -1, false)]
    [InlineData((AttendanceRequirementMode)99, 3, false)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, null, false)]
    [InlineData(null, 3, false)]
    public void Should_ValidateModeAndValueTogether_WhenUsingTypedRequirement(AttendanceRequirementMode? mode, int? value, bool expectedValid)
    {
        // Arrange
        var request = new UpdateRequirementRequestBody
        {
            StartingFrom = new DateOnly(2026, 11, 1), 
            Mode = mode, 
            Value = value
        };
        var validator = new UpdateRequirementRequestBodyValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        Assert.Equal(mode, request.Mode);
        Assert.Equal(value, request.Value);
    }

    [Fact]
    public void Should_RejectAmbiguousInput_WhenLegacyAndTypedRequirementsAreBothProvided()
    {
        // Arrange
        var request = new UpdateRequirementRequestBody
        {
            StartingFrom = new DateOnly(2026, 11, 1),
            Mode = AttendanceRequirementMode.DaysPerWeek,
            Value = 3, 
            RequiredOfficePercentage = 60
        };
        var validator = new UpdateRequirementRequestBodyValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }
}
