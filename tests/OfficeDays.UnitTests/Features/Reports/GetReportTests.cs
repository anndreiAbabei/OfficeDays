using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OfficeDays.Features.Reports.GetReport;
using OfficeDays.Features.Reports.GetReport.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Reports;

public sealed class GetReportTests
{
    [Theory]
    [InlineData(null, null, null, true)]
    [InlineData(2026, 1, 12, true)]
    [InlineData(2026, 6, 6, true)]
    [InlineData(0, 1, 12, false)]
    [InlineData(9999, 1, 12, false)]
    [InlineData(2026, 0, 12, false)]
    [InlineData(2026, 1, 13, false)]
    [InlineData(2026, 7, 6, false)]
    public void Validates_year_and_month_range(int? year, int? start, int? end, bool valid)
    {
        var request = new GetReportRequest { Year = year, StartMonth = start, EndMonth = end };
        Assert.Equal(valid, new GetReportRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void Maps_get_endpoint()
    {
        var executor = new RequestExecutor();
        var routes = Substitute.For<IEndpointRouteBuilder>();
        routes.DataSources.Returns([]);
        new GetReportEndpoint(executor).MapEndpoint(routes);
        Assert.Endpoint<GetReportRequest>(routes, executor, "/", HttpMethod.Get);
    }

    [Theory]
    [InlineData(ReportMonthState.NotStarted, "notStarted")]
    [InlineData(ReportMonthState.InProgress, "inProgress")]
    [InlineData(ReportMonthState.TargetMet, "targetMet")]
    [InlineData(ReportMonthState.BelowTarget, "belowTarget")]
    public void Maps_month_result_to_stable_api_values(ReportMonthState state, string expected)
    {
        var month = new AttendanceReportMonth(6, 20, 60, 12, null, null, state);
        var response = month.ToViewModel();
        Assert.Equal(expected, response.Result);
        Assert.Equal((6, 20, 60, 12), (response.Month, response.EligibleWorkingDays, response.RequiredOfficePercentage, response.RequiredOfficeDays));
        Assert.Null(response.OfficeDays);
        Assert.Null(response.AttendancePercentage);
    }

    [Fact]
    public void Logs_missing_user_and_successful_generation()
    {
        var logger = new FakeLogger<GetReportHandler>();
        var id = Guid.NewGuid();
        logger.LogUserNotFound(id);
        logger.Assert(LogLevel.Warning, $"User {id} was not found");
        logger.LogReportGenerated(id, 2026, 1, 12, 7);
        logger.Assert(LogLevel.Information, $"Generated attendance report for user {id}, year {2026}, months {1} to {12}, with {7} rows");
    }
}
