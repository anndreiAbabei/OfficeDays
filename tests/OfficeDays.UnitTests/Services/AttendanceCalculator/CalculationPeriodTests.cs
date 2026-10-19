using OfficeDays.Services;

namespace OfficeDays.UnitTests.Services;

public sealed class CalculationPeriodTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedStart = DateOnly.FromDateTime(DateTime.Now);
        var expectedEnd = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        const string expectedLabel = "Label";
        
        // act
        var period = new CalculationPeriod(expectedStart, expectedEnd, expectedLabel);

        // assert
        Assert.Equal(expectedStart, period.Start);
        Assert.Equal(expectedEnd, period.End);
        Assert.Equal(expectedLabel, period.Label);
    }

    [Fact]
    public void Should_SetProperties_Init()
    {
        // arrange
        var expectedStart = DateOnly.FromDateTime(DateTime.Now);
        var expectedEnd = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
        const string expectedLabel = "Label";
        
        // act
        var period = new CalculationPeriod(default, default, string.Empty)
        {
            Start = expectedStart,
            End = expectedEnd,
            Label = expectedLabel
        };

        // assert
        Assert.Equal(expectedStart, period.Start);
        Assert.Equal(expectedEnd, period.End);
        Assert.Equal(expectedLabel, period.Label);
    }
    
    [Fact]
    public void Should_CreatePeriod_UsingForMonth()
    {
        // arrange
        var date = DateTime.Now;
        var expectedStart = new DateOnly(date.Year, date.Month, 1);
        var expectedEnd = new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        var expectedLabel =  $"{date.Year:D4}-{date.Month:D2}"; ;
        
        // act
        var period = CalculationPeriod.ForMonth(date.Year, date.Month);

        // assert
        Assert.Equal(expectedStart, period.Start);
        Assert.Equal(expectedEnd, period.End);
        Assert.Equal(expectedLabel, period.Label);
    }
}
