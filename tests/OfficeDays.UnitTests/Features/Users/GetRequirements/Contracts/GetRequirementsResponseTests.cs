using OfficeDays.Features.Users.GetRequirements.Contracts;

namespace OfficeDays.UnitTests.Features.Users.GetRequirements.Contracts;

public sealed class GetRequirementsResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedToday = DateOnly.FromDateTime(DateTime.Now);
        var expectedItems = new List<GetRequirementsItem> { new GetRequirementsItem(Guid.NewGuid(), new DateOnly(2026, 10, 2), 23, new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero), true) };

        // act
        var sut = new GetRequirementsResponse(expectedToday, expectedItems);

        // assert
        Assert.Equal(expectedToday, sut.Today);
        Assert.Equal(expectedItems, sut.Items);
    }
}
