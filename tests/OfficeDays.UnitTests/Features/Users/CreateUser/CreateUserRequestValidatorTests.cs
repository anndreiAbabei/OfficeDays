using NSubstitute;
using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Features.Users.CreateUser;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Users.CreateUser;

public sealed class CreateUserRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var validator = new CreateUserRequestBodyValidator(dates);
        var sut = new CreateUserRequestValidator(validator);
        var request = new CreateUserRequest { Body = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var validator = new CreateUserRequestBodyValidator(dates);
        var sut = new CreateUserRequestValidator(validator);
        var request = new CreateUserRequest { Body = null! };

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
        var dates = Substitute.For<IUserDateService>();
        dates.IsValidTimeZone("Europe/Bucharest").Returns(true);
        var validator = new CreateUserRequestBodyValidator(dates);
        var sut = new CreateUserRequestValidator(validator);
        var request = new CreateUserRequest { Body = new CreateUserRequestBody { Username = "user", Password = "long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequiredOfficePercentage = 50 } with { Username = null! } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.Username");
    }
}
