using OfficeDays.Features.Users.CreateUser.Contracts;

namespace OfficeDays.UnitTests.Features.Users.CreateUser.Contracts;

public sealed class CreateUserResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedUsername = "Username value";
        var expectedTimeZoneId = "Europe/Bucharest";
        var expectedCountryCode = "RO";
        var expectedCountryName = "CountryName value";
        var expectedIsAdmin = true;
        var expectedEmail = "user@example.com";
        var expectedRequiredOfficePercentage = 28;

        // act
        var sut = new CreateUserResponse
        {
            Id = expectedId,
            Username = expectedUsername,
            TimeZoneId = expectedTimeZoneId,
            CountryCode = expectedCountryCode,
            CountryName = expectedCountryName,
            IsAdmin = expectedIsAdmin,
            Email = expectedEmail,
            RequiredOfficePercentage = expectedRequiredOfficePercentage
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUsername, sut.Username);
        Assert.Equal(expectedTimeZoneId, sut.TimeZoneId);
        Assert.Equal(expectedCountryCode, sut.CountryCode);
        Assert.Equal(expectedCountryName, sut.CountryName);
        Assert.Equal(expectedIsAdmin, sut.IsAdmin);
        Assert.Equal(expectedEmail, sut.Email);
        Assert.Equal(expectedRequiredOfficePercentage, sut.RequiredOfficePercentage);
    }
}
