using OfficeDays.Features.Authentication.Login.Contracts;

namespace OfficeDays.UnitTests.Features.Authentication.Login.Contracts;

public sealed class LoginResponseTests
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
        var expectedTeamDay = "TeamDay value";

        // act
        var sut = new LoginResponse(expectedId, expectedUsername, expectedTimeZoneId, expectedCountryCode, expectedCountryName, expectedIsAdmin, expectedEmail, expectedRequiredOfficePercentage, expectedTeamDay);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUsername, sut.Username);
        Assert.Equal(expectedTimeZoneId, sut.TimeZoneId);
        Assert.Equal(expectedCountryCode, sut.CountryCode);
        Assert.Equal(expectedCountryName, sut.CountryName);
        Assert.Equal(expectedIsAdmin, sut.IsAdmin);
        Assert.Equal(expectedEmail, sut.Email);
        Assert.Equal(expectedRequiredOfficePercentage, sut.RequiredOfficePercentage);
        Assert.Equal(expectedTeamDay, sut.TeamDay);
    }

    [Fact]
    public void Should_DefaultTeamDayToNull()
    {
        // arrange
        var expectedId = Guid.NewGuid();

        // act
        var sut = new LoginResponse(expectedId, "alice", "UTC", "RO", "Romania", false, null, 50);

        // assert
        Assert.Null(sut.TeamDay);
        Assert.Null(sut.Email);
    }
}
