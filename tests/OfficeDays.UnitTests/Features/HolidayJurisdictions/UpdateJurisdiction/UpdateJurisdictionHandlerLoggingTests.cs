using Microsoft.Extensions.Logging;
using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.UpdateJurisdiction;

public sealed class UpdateJurisdictionHandlerLoggingTests
{
    [Fact]
    public void Should_LogJurisdictionUpdated()
    {
        // arrange
        var expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<UpdateJurisdictionHandler>();

        // act
        logger.LogJurisdictionUpdated(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Information, $"Updated holiday jurisdiction {expectedJurisdictionCode}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogJurisdictionNotFound()
    {
        // arrange
        var expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<UpdateJurisdictionHandler>();

        // act
        logger.LogJurisdictionNotFound(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Warning, $"Holiday jurisdiction {expectedJurisdictionCode} was not found");
        Assert.Single(logger.Logs);
    }
}
