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
            Username = "user", 
            Password = "long-password", 
            TimeZoneId = "Europe/Bucharest", 
            CountryCode = "RO",
            Email = "user@example.com", 
            RequiredOfficePercentage = 50
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
            Username = null!,
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = "",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50 
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
            Username = "ab",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = new string('x', 101),
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = null!,
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = "short",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = "long-password",
            TimeZoneId = "invalid",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = "long-password",
            TimeZoneId = "",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "R",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "invalid",
            RequiredOfficePercentage = 50
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
    public void Should_ReturnValidationFailure_WhenRequiredOfficePercentageIsBelowZero()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { 
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = -1 
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "RequiredOfficePercentage");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenRequiredOfficePercentageIsAboveOneHundred()
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
            RequiredOfficePercentage = 101
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            error => error.PropertyName == "RequiredOfficePercentage");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenEmailIsNull()
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
            Email = null!,
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "",
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = " ",
            RequiredOfficePercentage = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenRequiredOfficePercentageIsZero()
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
            RequiredOfficePercentage = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenRequiredOfficePercentageIsOneHundred()
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
            RequiredOfficePercentage = 100
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenRequiredOfficePercentageIsNull()
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
            RequiredOfficePercentage = null
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
            Username = "abc",
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = new string('x', 100) ,
            Password = "long-password",
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
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
            Username = "user",
            Password = new string('x', 12) ,
            TimeZoneId = "Europe/Bucharest",
            CountryCode = "RO",
            Email = "user@example.com",
            RequiredOfficePercentage = 50
        };
        var sut = new CreateUserRequestBodyValidator(dates);

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 0, null, true)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 3, null, true)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 5, null, true)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 6, null, false)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, -1, null, false)]
    [InlineData(AttendanceRequirementMode.Percentage, 100, null, true)]
    [InlineData(AttendanceRequirementMode.Percentage, 101, null, false)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, null, null, false)]
    [InlineData(null, 3, null, false)]
    [InlineData((AttendanceRequirementMode)99, 3, null, false)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 3, 60, false)]
    [InlineData(null, null, null, true)]
    public void Should_ValidateTypedRequirement_WhenRegisteringUser(AttendanceRequirementMode? mode, int? value, int? legacyPercentage, bool expectedValid)
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
            Mode = mode, 
            Value = value, 
            RequiredOfficePercentage = legacyPercentage
        };
        var validator = new CreateUserRequestBodyValidator(dates);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        Assert.Equal(mode, request.Mode);
        Assert.Equal(value, request.Value);
    }
}
