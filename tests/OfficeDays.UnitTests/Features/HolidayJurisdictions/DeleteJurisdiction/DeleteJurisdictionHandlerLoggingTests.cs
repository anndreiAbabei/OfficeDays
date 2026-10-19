using Microsoft.Extensions.Logging;
using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.DeleteJurisdiction;

public sealed class DeleteJurisdictionHandlerLoggingTests
{
    [Fact]
    public void Should_LogJurisdictionDeleted()
    {
        // arrange
        var expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<DeleteJurisdictionHandler>();

        // act
        logger.LogJurisdictionDeleted(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Information, $"Deleted holiday jurisdiction {expectedJurisdictionCode}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogJurisdictionNotFound()
    {
        // arrange
        var expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<DeleteJurisdictionHandler>();

        // act
        logger.LogJurisdictionNotFound(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Warning, $"Holiday jurisdiction {expectedJurisdictionCode} was not found");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogJurisdictionInUse()
    {
        // arrange
        var expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<DeleteJurisdictionHandler>();

        // act
        logger.LogJurisdictionInUse(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Warning, $"Cannot delete holiday jurisdiction {expectedJurisdictionCode} because it is in use");
        Assert.Single(logger.Logs);
    }
}
