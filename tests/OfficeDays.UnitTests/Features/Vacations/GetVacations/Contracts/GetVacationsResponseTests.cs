using System.Collections;
using OfficeDays.Features.Vacations.GetVacations.Contracts;

namespace OfficeDays.UnitTests.Features.Vacations.GetVacations.Contracts;

public sealed class GetVacationsResponseTests
{
    [Fact]
    public void Should_EnumerateItems()
    {
        // arrange
        var expectedItems = new[]
        {
            new GetVacationsItem(Guid.NewGuid(),
            new DateOnly(2026, 10, 2),
            new DateOnly(2026, 10, 3),
            new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero)),
            new GetVacationsItem(Guid.NewGuid(),
            new DateOnly(2026, 10, 2),
            new DateOnly(2026, 10, 3),
            new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero))
        };
        var sut = new GetVacationsResponse(expectedItems);

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
            new GetVacationsItem(Guid.NewGuid(),
            new DateOnly(2026, 10, 2),
            new DateOnly(2026, 10, 3),
            new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero)),
            new GetVacationsItem(Guid.NewGuid(),
            new DateOnly(2026, 10, 2),
            new DateOnly(2026, 10, 3),
            new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero))
        };
        var sut = new GetVacationsResponse(expectedItems);

        // act
        var result = new List<GetVacationsItem>();
        var enumerator = ((IEnumerable)sut).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
                result.Add((GetVacationsItem)enumerator.Current!);
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
        var sut = new GetVacationsResponse([]);

        // act
        var result = sut.ToArray();

        // assert
        Assert.Empty(result);
    }
}
