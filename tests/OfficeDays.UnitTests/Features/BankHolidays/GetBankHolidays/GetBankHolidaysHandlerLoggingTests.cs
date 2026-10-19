using Microsoft.Extensions.Logging;
using OfficeDays.Features.BankHolidays.GetBankHolidays;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.BankHolidays.GetBankHolidays;

public sealed class GetBankHolidaysHandlerLoggingTests
{
    [Fact]
    public void Should_LogJurisdictionNotFound()
    {
        // arrange
        var expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<GetBankHolidaysHandler>();

        // act
        logger.LogJurisdictionNotFound(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Warning, $"Holiday jurisdiction {expectedJurisdictionCode} was not found");
        Assert.Single(logger.Logs);
    }
}
