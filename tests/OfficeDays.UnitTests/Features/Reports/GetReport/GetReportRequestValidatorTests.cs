using OfficeDays.Features.Reports.GetReport.Contracts;
using OfficeDays.Features.Reports.GetReport;

namespace OfficeDays.UnitTests.Features.Reports.GetReport;

public sealed class GetReportRequestValidatorTests
{
    [Theory]
    [InlineData(null, null, null, true)]
    [InlineData(1, 1, 12, true)]
    [InlineData(9998, 12, 12, true)]
    [InlineData(2026, null, 1, true)]
    [InlineData(2026, 12, null, true)]
    [InlineData(null, 3, 7, true)]
    [InlineData(0, 1, 12, false)]
    [InlineData(9999, 1, 12, false)]
    [InlineData(2026, 0, 12, false)]
    [InlineData(2026, 13, 12, false)]
    [InlineData(2026, 1, 0, false)]
    [InlineData(2026, 1, 13, false)]
    [InlineData(2026, 7, 6, false)]
    public void Should_ValidateYearAndMonthRange(int? year, int? startMonth, int? endMonth, bool expectedIsValid)
    {
        // arrange
        var request = new GetReportRequest { Year = year, StartMonth = startMonth, EndMonth = endMonth };
        var sut = new GetReportRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.Equal(expectedIsValid, result.IsValid);
    }
}
