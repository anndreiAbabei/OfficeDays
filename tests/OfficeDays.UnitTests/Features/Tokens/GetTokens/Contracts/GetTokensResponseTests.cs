using System.Collections;
using OfficeDays.Features.Tokens.GetTokens.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.GetTokens.Contracts;

public sealed class GetTokensResponseTests
{
    [Fact]
    public void Should_EnumerateItems()
    {
        // arrange
        var expectedItems = new[]
        {
            new GetTokensItem(Guid.NewGuid(), "Name value", new DateTimeOffset(2026, 10, 3, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 5, 9, 30, 0, TimeSpan.Zero), "Active"),
            new GetTokensItem(Guid.NewGuid(), "Name value", new DateTimeOffset(2026, 10, 3, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 5, 9, 30, 0, TimeSpan.Zero), "Active")
        };
        var sut = new GetTokensResponse(expectedItems);

        // act
        var result = sut.ToArray();

        // assert
        Assert.Equal(expectedItems, result);
    }

    [Fact]
    public void Should_EnumerateItems_ThroughNonGenericInterface()
    {
        // arrange
        var expectedItems = new[]
        {
            new GetTokensItem(Guid.NewGuid(), "Name value", new DateTimeOffset(2026, 10, 3, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 5, 9, 30, 0, TimeSpan.Zero), "Active"),
            new GetTokensItem(Guid.NewGuid(), "Name value", new DateTimeOffset(2026, 10, 3, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 5, 9, 30, 0, TimeSpan.Zero), "Active")
        };
        var sut = new GetTokensResponse(expectedItems);

        // act
        var result = new List<GetTokensItem>();
        var enumerator = ((IEnumerable)sut).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
                result.Add((GetTokensItem)enumerator.Current!);
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }

        // assert
        Assert.Equal(expectedItems, result);
    }

    [Fact]
    public void Should_EnumerateEmptyItems()
    {
        // arrange
        var sut = new GetTokensResponse([]);

        // act
        var result = sut.ToArray();

        // assert
        Assert.Empty(result);
    }
}
