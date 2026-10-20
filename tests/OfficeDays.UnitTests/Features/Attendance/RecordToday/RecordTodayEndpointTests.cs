using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Attendance.RecordToday;
using OfficeDays.Features.Attendance.RecordToday.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Attendance.RecordToday;

public sealed class RecordTodayEndpointTests
{
    [Fact]
    public void Should_MapPostEndpoint_WhenMappingRoutes()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new RecordTodayEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<RecordTodayRequest>(epb, executor, "/", HttpMethod.Post);
    }
}
