using Microsoft.Extensions.Logging;
using OfficeDays.Features.BankHolidays.ReplaceBankHolidays;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.BankHolidays.ReplaceBankHolidays;

public sealed class ReplaceBankHolidaysHandlerLoggingTests
{
    [Fact]
    public void Should_LogHolidaysReplaced()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedJurisdictionCode = "jurisdictionCode value";
        var expectedYear = 23;
        var expectedHolidayCount = 24;
        var logger = new FakeLogger<ReplaceBankHolidaysHandler>();

        // act
        logger.LogHolidaysReplaced(expectedUserId, expectedJurisdictionCode, expectedYear, expectedHolidayCount);

        // assert
        logger.Assert(LogLevel.Information, $"Admin user {expectedUserId} replaced bank holidays for {expectedJurisdictionCode} in {expectedYear} with {expectedHolidayCount} entries");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogJurisdictionNotFound()
    {
        // arrange
        var expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<ReplaceBankHolidaysHandler>();

        // act
        logger.LogJurisdictionNotFound(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Warning, $"Holiday jurisdiction {expectedJurisdictionCode} was not found");
        Assert.Single(logger.Logs);
    }
}
