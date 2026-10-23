using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateRequirement.Contracts;
using OfficeDays.Features.Users.CreateRequirement;

namespace OfficeDays.UnitTests.Features.Users.CreateRequirement;

public sealed class CreateRequirementRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var request = new CreateRequirementRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            StartingFrom = new DateOnly(2026, 10, 1), 
            RequirementValue = 50
        };
        var sut = new CreateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenValueIsBelowZero()
    {
        // arrange
        var request = new CreateRequirementRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            StartingFrom = DateOnly.FromDateTime(DateTime.Now), 
            RequirementValue = -1
        };
        var sut = new CreateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "RequirementValue");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenValueIsAboveOneHundred()
    {
        // arrange
        var request = new CreateRequirementRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            StartingFrom = DateOnly.FromDateTime(DateTime.Now), 
            RequirementValue = 101
        };
        var sut = new CreateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "RequirementValue");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenStartingFromIsNotFirstDayOfMonth()
    {
        // arrange
        var request = new CreateRequirementRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            StartingFrom = new DateOnly(2026, 10, 2), 
            RequirementValue = 50
        };
        var sut = new CreateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "StartingFrom");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenValueIsZero()
    {
        // arrange
        var request = new CreateRequirementRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            StartingFrom = new DateOnly(2026, 10, 1),
            RequirementValue = 0
        } ;
        var sut = new CreateRequirementRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenValueIsOneHundred()
    {
        // arrange
        var request = new CreateRequirementRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            StartingFrom = new DateOnly(2026, 10, 1),
            RequirementValue = 100
        };
        var sut = new CreateRequirementRequestBodyValidator();

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
        var request = new CreateRequirementRequestBody { StartingFrom = new DateOnly(2026, 11, 1), RequirementMode = mode, RequirementValue = value };
        var validator = new CreateRequirementRequestBodyValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        Assert.Equal(mode, request.RequirementMode);
        Assert.Equal(value, request.RequirementValue);
    }

}
