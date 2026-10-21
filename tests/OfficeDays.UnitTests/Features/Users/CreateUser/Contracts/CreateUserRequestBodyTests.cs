using OfficeDays.Features.Users.CreateUser.Contracts;

namespace OfficeDays.UnitTests.Features.Users.CreateUser.Contracts;

public sealed class CreateUserRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const string expectedUsername = "Username value";
        const string expectedPassword = "a-long-password";
        const string expectedTimeZoneId = "Europe/Bucharest";
        const string expectedCountryCode = "RO";
        const string expectedEmail = "user@example.com";
        const int expectedRequiredOfficePercentage = 26;

        // act
        var sut = new CreateUserRequestBody
        {
            Username = expectedUsername,
            Password = expectedPassword,
            TimeZoneId = expectedTimeZoneId,
            CountryCode = expectedCountryCode,
            Email = expectedEmail,
            RequiredOfficePercentage = expectedRequiredOfficePercentage
        };

        // assert
        Assert.Equal(expectedUsername, sut.Username);
        Assert.Equal(expectedPassword, sut.Password);
        Assert.Equal(expectedTimeZoneId, sut.TimeZoneId);
        Assert.Equal(expectedCountryCode, sut.CountryCode);
        Assert.Equal(expectedEmail, sut.Email);
        Assert.Equal(expectedRequiredOfficePercentage, sut.RequiredOfficePercentage);
    }

    [Fact]
    public void Should_LeaveRequirementUnspecified_WhenNoRequirementIsProvided()
    {
        // arrange
        int? expectedPercentage = null;

        // act
        var sut = new CreateUserRequestBody
        {
            Username = "alice",
            Password = "long-password", 
            TimeZoneId = "UTC",
            CountryCode = "RO"
        };

        // assert
        Assert.Equal(expectedPercentage, sut.RequiredOfficePercentage);
        Assert.Null(sut.Email);
    }

    [Fact]
    public void Should_PreserveNullRequiredOfficePercentage()
    {
        // arrange
        int? expectedPercentage = null;

        // act
        var sut = new CreateUserRequestBody
        {
            Username = "alice", 
            Password = "long-password", 
            TimeZoneId = "UTC", 
            CountryCode = "RO", 
            RequiredOfficePercentage = expectedPercentage
        };

        // assert
        Assert.Null(sut.RequiredOfficePercentage);
    }
}
