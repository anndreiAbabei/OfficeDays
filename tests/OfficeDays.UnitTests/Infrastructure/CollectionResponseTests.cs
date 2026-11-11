using System.Diagnostics.CodeAnalysis;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Infrastructure;

public sealed class CollectionResponseTests
{
    [Fact]
    public void Should_ContainsSameItems()
    {
        // arrange
        var items = new[] { 1, 2, 3 };

        // act
        var sut = new TestCollectionResponse(items);

        // assert
        Assert.Equal(items, sut);
    }

    [Fact]
    public void Should_PreserveItems_WhenSourceListChanges()
    {
        // Arrange
        int[] expectedItems = [1, 2, 3];
        var items = new List<int>(expectedItems);
        var sut = new TestCollectionResponse(items);

        // Act
        items.Clear();

        // Assert
        Assert.Equal(expectedItems, sut);
    }

    [Fact]
    public void Should_PreserveItems_WhenSourceArrayChanges()
    {
        // Arrange
        int[] expectedItems = [1, 2, 3];
        int[] items = [1, 2, 3];
        var sut = new TestCollectionResponse(items);

        // Act
        items[0] = 4;

        // Assert
        Assert.Equal(expectedItems, sut);
    }

    [Fact]
    public void Should_PreserveProjectedItems_WhenDeferredSourceChanges()
    {
        // Arrange
        int[] expectedItems = [2, 4, 6];
        var items = new List<int> { 1, 2, 3 };
        var sut = new TestCollectionResponse(items.Select(item => item * 2));

        // Act
        items.Clear();

        // Assert
        Assert.Equal(expectedItems, sut);
    }

    [ExcludeFromCodeCoverage]
    [SuppressMessage("Design", "CA1067:Override Object.Equals(object) when implementing IEquatable<T>")]
    private sealed record TestCollectionResponse : CollectionResponse<int>
    {
        public TestCollectionResponse(IEnumerable<int> items) : base(items) {}
    }
}
