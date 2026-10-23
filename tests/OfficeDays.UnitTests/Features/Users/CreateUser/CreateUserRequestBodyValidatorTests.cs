using NSubstitute;
using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Features.Users.CreateUser;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Users.CreateUser;

public sealed class CreateUserRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user", 
            Password = "long-password", 
            TimeZoneId = "Europe/Bucharest", 
            CountryCode = "RO",
            Email = "user@example.com", 
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenUsernameIsNull()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = null!,
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenUsernameIsEmpty()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody 
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenUsernameIsTooShort()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "ab",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenUsernameIsTooLong()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = new string('x', 101),
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenPasswordIsNull()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = null!,
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "Password");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenPasswordIsTooShort()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "short",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenTimeZoneIdIsMalformed()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "invalid",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "TimeZoneId");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenTimeZoneIdIsEmpty()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "TimeZoneId");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCountryCodeIsTooShort()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "R",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "CountryCode");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCountryCodeIsEmpty()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "CountryCode");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenEmailIsMalformed()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "invalid",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "Email");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenValueIsBelowZero()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = -1
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "RequirementValue");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenValueIsAboveOneHundred()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 101
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "RequirementValue");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenEmailIsNull()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = null!,
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenEmailIsEmpty()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenEmailIsWhitespace()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = " ",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenValueIsZero()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenValueIsOneHundred()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 100
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenValueIsNull()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = null
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenUsernameIsMinimumLength()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "abc",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenUsernameIsMaximumLength()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = new string('x', 100) ,
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenPasswordIsMinimumLength()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = "user",
            Password = new string('x', 12) ,
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequirementValue = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 0, true)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 3, true)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 5, true)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 6, false)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, -1, false)]
    [InlineData(AttendanceRequirementMode.Percentage, 100, true)]
    [InlineData(AttendanceRequirementMode.Percentage, 101, false)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, null, false)]
    [InlineData(null, 3, false)]
    [InlineData((AttendanceRequirementMode)99, 3, false)]
    [InlineData(null, null, true)]
    public void Should_ValidateTypedRequirement_WhenRegisteringUser(AttendanceRequirementMode? mode, int? value, bool expectedValid)
    {
        // Arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("UTC").Returns(true);
        var request = new CreateUserRequestBody
        {
            Username = "weekly-user", 
            Password = "long-password", 
            TimeZoneId = "UTC", 
            CountryCode = "RO",
            RequirementMode = mode,
            RequirementValue = value
        };
        var validator = new CreateUserRequestBodyValidator(dates);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        Assert.Equal(mode, request.RequirementMode);
        Assert.Equal(value, request.RequirementValue);
    }
}
