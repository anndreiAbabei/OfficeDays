using System.Collections;
using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;

namespace OfficeDays.UnitTests.Features.BankHolidays.GetBankHolidays.Contracts;

public sealed class GetBankHolidaysResponseTests
{
    [Fact]
    public void Should_EnumerateItems()
    {
        // arrange
        var expectedItems = new[]
        {
            new GetBankHolidayItem("RO", new DateOnly(2026, 10, 2), "Name value"),
            new GetBankHolidayItem("RO", new DateOnly(2026, 10, 2), "Name value")
        };
        var sut = new GetBankHolidaysResponse(expectedItems);

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
            new GetBankHolidayItem("RO", new DateOnly(2026, 10, 2), "Name value"),
            new GetBankHolidayItem("RO", new DateOnly(2026, 10, 2), "Name value")
        };
        var sut = new GetBankHolidaysResponse(expectedItems);

        // act
        var result = new List<GetBankHolidayItem>();
        var enumerator = ((IEnumerable)sut).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
                result.Add((GetBankHolidayItem)enumerator.Current!);
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
        var sut = new GetBankHolidaysResponse([]);

        // act
        var result = sut.ToArray();

        // assert
        Assert.Empty(result);
    }

    [Fact]
    public void Should_SnapshotItems_WhenSourceChanges()
    {
        // Arrange
        var expectedItem = new GetBankHolidayItem("RO", new DateOnly(2026, 1, 1), "New Year");
        var items = new List<GetBankHolidayItem> { expectedItem };
        var sut = new GetBankHolidaysResponse(items);

        // Act
        items.Clear();

        // Assert
        Assert.Equal(expectedItem, Assert.Single(sut));
    }
}
