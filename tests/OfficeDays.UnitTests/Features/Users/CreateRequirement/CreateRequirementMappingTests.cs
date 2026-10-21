using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateRequirement;

namespace OfficeDays.UnitTests.Features.Users.CreateRequirement;

public sealed class CreateRequirementMappingTests
{
    [Theory]
    [InlineData("0001-01-01", true)]
    [InlineData("2026-10-01", false)]
    public void Should_MapToViewModel(string startingFrom, bool expectedIsBaseline)
    {
        // arrange
        var sut = new UserAttendanceRequirement
        {
            Id = Guid.NewGuid(), 
            UserId = Guid.NewGuid(), 
            StartingFrom = DateOnly.Parse(startingFrom), 
            Value = 65,
            CreatedAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero)
        };

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
    public void Should_PreserveWeeklyModeAndValue_WhenMappingRequirement()
    {
        // Arrange
        const int expectedValue = 3;
        const int expectedPercentage = 60;
        const AttendanceRequirementMode expectedMode = AttendanceRequirementMode.DaysPerWeek;
        var row = new UserAttendanceRequirement
        {
            Id = Guid.NewGuid(), 
            UserId = Guid.NewGuid(), 
            StartingFrom = new DateOnly(2026, 11, 1),
            CreatedAt = new DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.Zero),
            Mode = AttendanceRequirementMode.DaysPerWeek, Value = expectedValue
        };

        // Act
        var result = row.ToViewModel();

        // Assert
        Assert.Equal(row.Id, result.Id);
        Assert.Equal(row.StartingFrom, result.StartingFrom);
        Assert.Equal(row.CreatedAt, result.CreatedAt);
        Assert.False(result.IsBaseline);
        Assert.Equal(expectedMode, result.Mode);
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedPercentage, result.RequiredOfficePercentage);
    }
}
