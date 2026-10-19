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
        var sut = new UserRequiredOfficePercentage
        {
            Id = Guid.NewGuid(), 
            UserId = Guid.NewGuid(), 
            StartingFrom = DateOnly.Parse(startingFrom), 
            RequiredOfficePercentage = 65, 
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
}
