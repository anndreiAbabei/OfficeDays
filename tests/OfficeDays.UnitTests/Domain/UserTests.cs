using OfficeDays.Domain;

namespace OfficeDays.UnitTests.Domain;

public sealed class UserTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        const string expectedUsername = "Test";
        const string expectedNormalizedUsername = "test";
        const string expectedEmail = "test@office.local";
        const int expectedRequiredOfficePercentage = 26;
        const string expectedPasswordHash = "test-password";
        const string expectedTimeZoneId = "ro";
        const int expectedHolidayJurisdictionId = 123;
        const bool expectedIsAdmin = true;
        var expectedCreatedAt = DateTime.Now;
        var expectedHolidayJurisdiction = new HolidayJurisdiction
        {
            Code = null!,
            Name = null!
        };
        var expectedAttendances = new List<Attendance>();
        var expectedVacations = new List<Vacation>();
        var expectedApiTokens = new List<ApiToken>();
        
        // act
        var sut = new User
        {
            Id = expectedId,
            Username = expectedUsername,
            NormalizedUsername = expectedNormalizedUsername,
            Email = expectedEmail,
            RequiredOfficePercentage = expectedRequiredOfficePercentage,
            PasswordHash = expectedPasswordHash,
            TimeZoneId = expectedTimeZoneId,
            HolidayJurisdictionId = expectedHolidayJurisdictionId,
            IsAdmin = expectedIsAdmin,
            CreatedAt = expectedCreatedAt,
            HolidayJurisdiction = expectedHolidayJurisdiction,
            Attendances = expectedAttendances,
            Vacations = expectedVacations,
            ApiTokens = expectedApiTokens
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUsername, sut.Username);
        Assert.Equal(expectedNormalizedUsername, sut.NormalizedUsername);
        Assert.Equal(expectedEmail, sut.Email);
        Assert.Equal(expectedRequiredOfficePercentage, sut.RequiredOfficePercentage);
        Assert.Equal(expectedPasswordHash, sut.PasswordHash);
        Assert.Equal(expectedTimeZoneId, sut.TimeZoneId);
        Assert.Equal(expectedHolidayJurisdictionId, sut.HolidayJurisdictionId);
        Assert.Equal(expectedIsAdmin, sut.IsAdmin);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
        Assert.Same(expectedHolidayJurisdiction, sut.HolidayJurisdiction);
        Assert.Same(expectedAttendances, sut.Attendances);
        Assert.Same(expectedVacations, sut.Vacations);
        Assert.Same(expectedApiTokens, sut.ApiTokens);
    }

    [Fact]
    public void Should_HaveDefaultValueForRequiredOfficePercentage()
    {
        // arrange
        const int expectedRequiredOfficePercentage = 50;
        var sut = new User
        {
            Username = null!,
            NormalizedUsername = null!,
            PasswordHash = null!,
            TimeZoneId = null!
        };

        // act
        var result = sut.RequiredOfficePercentage;

        // assert
        Assert.Equal(expectedRequiredOfficePercentage, result);
    }
}
