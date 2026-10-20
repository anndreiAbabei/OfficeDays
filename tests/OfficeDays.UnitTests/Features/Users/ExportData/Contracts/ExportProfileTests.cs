using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportProfileTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        const string expectedUsername = "User";
        const string expectedNormalizedUsername = "USER";
        const string expectedEmail = "user@example.com";
        const string expectedTimeZoneId = "Europe/Bucharest";
        const int expectedHolidayJurisdictionId = 42;
        const bool expectedIsAdmin = true;
        var expectedCreatedAt = new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero);

        // act
        var sut = new ExportProfile(expectedId, expectedUsername, expectedNormalizedUsername, expectedEmail, expectedTimeZoneId, expectedHolidayJurisdictionId, expectedIsAdmin, expectedCreatedAt);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUsername, sut.Username);
        Assert.Equal(expectedNormalizedUsername, sut.NormalizedUsername);
        Assert.Equal(expectedEmail, sut.Email);
        Assert.Equal(expectedTimeZoneId, sut.TimeZoneId);
        Assert.Equal(expectedHolidayJurisdictionId, sut.HolidayJurisdictionId);
        Assert.Equal(expectedIsAdmin, sut.IsAdmin);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
    }
}
