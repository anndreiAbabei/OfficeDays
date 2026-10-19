using Microsoft.Extensions.Logging;
using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.CreateJurisdiction;

public sealed class CreateJurisdictionHandlerLoggingTests
{
    [Fact]
    public void Should_LogDuplicateJurisdiction()
    {
        // arrange
        const string expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<CreateJurisdictionHandler>();

        // act
        logger.LogDuplicateJurisdiction(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Warning, $"A duplicate holiday jurisdiction was attempted for {expectedJurisdictionCode}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogJurisdictionCreated()
    {
        // arrange
        const string expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<CreateJurisdictionHandler>();

        // act
        logger.LogJurisdictionCreated(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Information, $"Created holiday jurisdiction {expectedJurisdictionCode}");
        Assert.Single(logger.Logs);
    }
}
