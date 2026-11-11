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

    [ExcludeFromCodeCoverage]
    [SuppressMessage("Design", "CA1067:Override Object.Equals(object) when implementing IEquatable<T>")]
    private sealed record TestCollectionResponse : CollectionResponse<int>
    {
        public TestCollectionResponse(IEnumerable<int> items) : base(items) {}
    }
}
