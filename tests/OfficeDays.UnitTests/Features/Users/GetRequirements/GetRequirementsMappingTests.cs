using NSubstitute;
using OfficeDays.Domain;
using OfficeDays.Features.Users.GetRequirements;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Users.GetRequirements;

public sealed class GetRequirementsMappingTests
{
    [Theory]
    [InlineData("0001-01-01", true)]
    [InlineData("2026-10-01", false)]
    public void Should_MapToViewModel(string startingFrom, bool expectedIsBaseline)
    {
        // arrange
        var sut = new UserRequiredOfficePercentage { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StartingFrom = DateOnly.Parse(startingFrom), RequiredOfficePercentage = 65, CreatedAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero) };

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Equal(sut.Id, result.Id);
        Assert.Equal(sut.StartingFrom, result.StartingFrom);
        Assert.Equal(sut.RequiredOfficePercentage, result.RequiredOfficePercentage);
        Assert.Equal(sut.CreatedAt, result.CreatedAt);
        Assert.Equal(expectedIsBaseline, result.IsBaseline);
    }

    [Fact]
    public void Should_MapUserDateAndRequirementsInSourceOrder()
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
        var expectedToday = new DateOnly(2026, 10, 19);
        var dates = Substitute.For<IUserDateService>();
        dates.Today(sut).Returns(expectedToday);
        UserRequiredOfficePercentage[] requirements =
        [
            new() { Id = Guid.NewGuid(), StartingFrom = expectedToday, RequiredOfficePercentage = 65 },
            new() { Id = Guid.NewGuid(), StartingFrom = DateOnly.MinValue, RequiredOfficePercentage = 50 }
        ];

        // act
        var result = sut.ToViewModel(requirements, dates);

        // assert
        Assert.Equal(expectedToday, result.Today);
        Assert.Equal(requirements.Select(row => row.Id), result.Items.Select(item => item.Id));
        Assert.False(result.Items.First().IsBaseline);
        Assert.True(result.Items.Last().IsBaseline);
        dates.Received(1).Today(sut);
    }

    [Fact]
    public void Should_MapEmptyRequirements()
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
        var expectedToday = new DateOnly(2026, 10, 19);
        var dates = Substitute.For<IUserDateService>();
        dates.Today(sut).Returns(expectedToday);

        // act
        var result = sut.ToViewModel([], dates);

        // assert
        Assert.Equal(expectedToday, result.Today);
        Assert.Empty(result.Items);
    }
}
