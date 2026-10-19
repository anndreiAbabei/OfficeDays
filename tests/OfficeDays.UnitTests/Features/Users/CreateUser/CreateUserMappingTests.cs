using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateUser;

namespace OfficeDays.UnitTests.Features.Users.CreateUser;

public sealed class CreateUserMappingTests
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
            Preferences = new UserPreferences { TeamDay = DayOfWeek.Wednesday },
            RequiredOfficePercentages = [new UserRequiredOfficePercentage { RequiredOfficePercentage = 65 }]
        };

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Equal(sut.Id, result.Id);
        Assert.Equal(sut.Username, result.Username);
        Assert.Equal(sut.TimeZoneId, result.TimeZoneId);
        Assert.Equal(sut.IsAdmin, result.IsAdmin);
        Assert.Equal(sut.Email, result.Email);
        Assert.Equal(sut.HolidayJurisdiction.Code, result.CountryCode);
        Assert.Equal(sut.HolidayJurisdiction.Name, result.CountryName);
        Assert.Equal(expectedPercentage, result.RequiredOfficePercentage);
    }
}
