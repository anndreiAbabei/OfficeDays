using OfficeDays.Domain;
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
        const int expectedValue = 26;

        // act
        var sut = new CreateUserRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            Username = expectedUsername,
            Password = expectedPassword,
            TimeZoneId = expectedTimeZoneId,
            CountryCode = expectedCountryCode,
            Email = expectedEmail,
            RequirementValue = expectedValue
        };

        // assert
        Assert.Equal(expectedUsername, sut.Username);
        Assert.Equal(expectedPassword, sut.Password);
        Assert.Equal(expectedTimeZoneId, sut.TimeZoneId);
        Assert.Equal(expectedCountryCode, sut.CountryCode);
        Assert.Equal(expectedEmail, sut.Email);
        Assert.Equal(expectedValue, sut.RequirementValue);
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
        Assert.Equal(expectedPercentage, sut.RequirementValue);
        Assert.Null(sut.Email);
    }

    [Fact]
    public void Should_PreserveNullValue()
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
            RequirementValue = expectedPercentage
        };

        // assert
        Assert.Null(sut.RequirementValue);
    }
}
