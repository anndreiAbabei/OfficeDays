using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportRequirementTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var expectedStartingFrom = DateOnly.MinValue;
        const int expectedRequiredOfficePercentage = 75;
        var expectedCreatedAt = new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero);

        // act
        var sut = new ExportRequirement(expectedId, expectedUserId, expectedStartingFrom, expectedRequiredOfficePercentage, expectedCreatedAt);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUserId, sut.UserId);
        Assert.Equal(expectedStartingFrom, sut.StartingFrom);
        Assert.Equal(expectedRequiredOfficePercentage, sut.RequiredOfficePercentage);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
    }
}
