using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportVacationTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var expectedFrom = new DateOnly(2026, 10, 19);
        var expectedTo = new DateOnly(2026, 10, 23);
        var expectedCreatedAt = new DateTimeOffset(2026, 10, 19, 9, 30, 0, TimeSpan.Zero);

        // act
        var sut = new ExportVacation(expectedId, expectedUserId, expectedFrom, expectedTo, expectedCreatedAt);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUserId, sut.UserId);
        Assert.Equal(expectedFrom, sut.From);
        Assert.Equal(expectedTo, sut.To);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
    }
}
