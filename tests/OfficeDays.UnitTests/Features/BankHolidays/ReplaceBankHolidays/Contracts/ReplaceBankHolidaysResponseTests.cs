using System.Collections;
using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;

namespace OfficeDays.UnitTests.Features.BankHolidays.ReplaceBankHolidays.Contracts;

public sealed class ReplaceBankHolidaysResponseTests
{
    [Fact]
    public void Should_EnumerateItems()
    {
        // arrange
        var expectedItems = new[]
        {
            new ReplaceBankHolidaysItem("RO", new DateOnly(2026, 10, 2), "Name value"),
            new ReplaceBankHolidaysItem("RO", new DateOnly(2026, 10, 2), "Name value")
        };
        var sut = new ReplaceBankHolidaysResponse(expectedItems);

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
            new ReplaceBankHolidaysItem("RO", new DateOnly(2026, 10, 2), "Name value"),
            new ReplaceBankHolidaysItem("RO", new DateOnly(2026, 10, 2), "Name value")
        };
        var sut = new ReplaceBankHolidaysResponse(expectedItems);

        // act
        var result = new List<ReplaceBankHolidaysItem>();
        var enumerator = ((IEnumerable)sut).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
                result.Add((ReplaceBankHolidaysItem)enumerator.Current!);
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
        var sut = new ReplaceBankHolidaysResponse([]);

        // act
        var result = sut.ToArray();

        // assert
        Assert.Empty(result);
    }

    [Fact]
    public void Should_SnapshotItems_WhenSourceChanges()
    {
        // Arrange
        var expectedItem = new ReplaceBankHolidaysItem("RO", new DateOnly(2026, 1, 1), "New Year");
        var items = new List<ReplaceBankHolidaysItem> { expectedItem };
        var sut = new ReplaceBankHolidaysResponse(items);

        // Act
        items.Clear();

        // Assert
        Assert.Equal(expectedItem, Assert.Single(sut));
    }
}
