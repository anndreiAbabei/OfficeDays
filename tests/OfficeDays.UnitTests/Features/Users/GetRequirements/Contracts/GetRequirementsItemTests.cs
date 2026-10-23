using OfficeDays.Features.Users.GetRequirements.Contracts;

namespace OfficeDays.UnitTests.Features.Users.GetRequirements.Contracts;

public sealed class GetRequirementsItemTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedStartingFrom = new DateOnly(2026, 10, 2);
        var expectedValue = 23;
        var expectedCreatedAt = new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero);
        var expectedIsBaseline = true;

        // act
        var sut = new GetRequirementsItem(expectedId, expectedStartingFrom, expectedValue, expectedCreatedAt, expectedIsBaseline);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedStartingFrom, sut.StartingFrom);
        Assert.Equal(expectedValue, sut.RequirementValue);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
        Assert.Equal(expectedIsBaseline, sut.IsBaseline);
    }
}
