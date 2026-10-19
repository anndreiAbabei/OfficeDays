using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Reports.GetReport.Contracts;
using OfficeDays.Features.Reports.GetReport;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Reports.GetReport;

public sealed class GetReportEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetReportEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<GetReportRequest>(epb, executor, "/", HttpMethod.Get);
    }
}
