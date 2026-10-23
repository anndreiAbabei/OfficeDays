using OfficeDays.Domain;
using OfficeDays.Features.Authentication.GetCurrentUser;

namespace OfficeDays.UnitTests.Features.Authentication.GetCurrentUser;

public sealed class GetCurrentUserMappingTests
{
    [Fact]
    public void Should_MapToViewModel()
    {
        // arrange
        var expectedPercentage = 65;
        var sut = new User
        {
            Id = Guid.NewGuid(),
            Username = "alice",
            NormalizedUsername = "ALICE",
            PasswordHash = "hash",
            TimeZoneId = "Europe/Bucharest",
            HolidayJurisdiction = new HolidayJurisdiction { Id = 7, Code = "RO", Name = "Romania" },
            IsAdmin = true,
            Email = "alice@example.com",
            Preferences = new UserPreferences { TeamDay = DayOfWeek.Wednesday }
        };

        // act
        var result = sut.ToViewModel(new UserAttendanceRequirement { RequirementMode = AttendanceRequirementMode.Percentage, RequirementValue = expectedPercentage });

        // assert
        Assert.Equal(sut.Id, result.Id);
        Assert.Equal(sut.Username, result.Username);
        Assert.Equal(sut.TimeZoneId, result.TimeZoneId);
        Assert.Equal(sut.IsAdmin, result.IsAdmin);
        Assert.Equal(sut.Email, result.Email);
        Assert.Equal(sut.HolidayJurisdiction.Code, result.CountryCode);
        Assert.Equal(sut.HolidayJurisdiction.Name, result.CountryName);
        Assert.Equal(expectedPercentage, result.RequirementValue);
        Assert.Equal("wednesday", result.TeamDay);
    }

    [Fact]
    public void Should_PreserveNullEmailAndTeamDay()
    {
        // arrange
        var sut = new User
        {
            Id = Guid.NewGuid(),
            Username = "alice",
            NormalizedUsername = "ALICE",
            PasswordHash = "hash",
            TimeZoneId = "Europe/Bucharest",
            HolidayJurisdiction = new HolidayJurisdiction { Id = 7, Code = "RO", Name = "Romania" },
            IsAdmin = true,
            Email = "alice@example.com",
            Preferences = new UserPreferences { TeamDay = DayOfWeek.Wednesday }
        };
        sut.Email = null;
        sut.Preferences.TeamDay = null;

        // act
        var result = sut.ToViewModel(new UserAttendanceRequirement { RequirementMode = AttendanceRequirementMode.Percentage, RequirementValue = 0 });

        // assert
        Assert.Null(result.Email);
        Assert.Null(result.TeamDay);
        Assert.Equal(0, result.RequirementValue);
    }
}
