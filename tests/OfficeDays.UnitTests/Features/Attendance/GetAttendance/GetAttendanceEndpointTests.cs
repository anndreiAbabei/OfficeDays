using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Attendance.GetAttendance;
using OfficeDays.Features.Attendance.GetAttendance.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Attendance.GetAttendance;

public sealed class GetAttendanceEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetAttendanceEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<GetAttendanceRequest>(epb, executor, "/", HttpMethod.Get);
    }
}
