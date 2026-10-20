using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportPlanTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var expectedDate = new DateOnly(2026, 10, 19);
        var expectedCreatedAt = new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero);

        // act
        var sut = new ExportPlan(expectedId, expectedUserId, expectedDate, expectedCreatedAt);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUserId, sut.UserId);
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
    }
}
