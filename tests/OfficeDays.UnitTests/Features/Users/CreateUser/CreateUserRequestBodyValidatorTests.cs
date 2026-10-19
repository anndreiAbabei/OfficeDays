using NSubstitute;
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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Username = null! };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Username = "" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenUsernameIsTooShort()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Username = "ab" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenUsernameIsTooLong()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Username = new string('x', 101) };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenPasswordIsNull()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Password = null! };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenPasswordIsTooShort()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Password = "short" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { TimeZoneId = "invalid" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TimeZoneId");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenTimeZoneIdIsEmpty()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { TimeZoneId = "" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TimeZoneId");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCountryCodeIsTooShort()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { CountryCode = "R" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "CountryCode");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCountryCodeIsEmpty()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { CountryCode = "" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "CountryCode");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenEmailIsMalformed()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Email = "invalid" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Email");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenRequiredOfficePercentageIsBelowZero()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { RequiredOfficePercentage = -1 };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { RequiredOfficePercentage = 101 };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "RequiredOfficePercentage");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenEmailIsNull()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Email = null };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Email = "" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Email = " " };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { RequiredOfficePercentage = 0 };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { RequiredOfficePercentage = 100 };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { RequiredOfficePercentage = null };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Username = "abc" };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Username = new string('x', 100) };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

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
        var request = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Password = new string('x', 12) };
        var sut = new CreateUserRequestBodyValidator(dates, Substitute.For<IUserService>());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
