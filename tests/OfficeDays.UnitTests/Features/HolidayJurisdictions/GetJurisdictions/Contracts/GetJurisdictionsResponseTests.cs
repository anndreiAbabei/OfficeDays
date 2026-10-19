using System.Collections;
using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.GetJurisdictions.Contracts;

public sealed class GetJurisdictionsResponseTests
{
    [Fact]
    public void Should_EnumerateItems()
    {
        // arrange
        var expectedItems = new[]
        {
            new GetJurisdictionsItem("RO", "Name value"),
            new GetJurisdictionsItem("RO", "Name value")
        };
        var sut = new GetJurisdictionsResponse(expectedItems);

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
            new GetJurisdictionsItem("RO", "Name value"),
            new GetJurisdictionsItem("RO", "Name value")
        };
        var sut = new GetJurisdictionsResponse(expectedItems);

        // act
        var result = new List<GetJurisdictionsItem>();
        var enumerator = ((IEnumerable)sut).GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
                result.Add((GetJurisdictionsItem)enumerator.Current!);
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
        var sut = new GetJurisdictionsResponse([]);

        // act
        var result = sut.ToArray();

        // assert
        Assert.Empty(result);
    }
}
