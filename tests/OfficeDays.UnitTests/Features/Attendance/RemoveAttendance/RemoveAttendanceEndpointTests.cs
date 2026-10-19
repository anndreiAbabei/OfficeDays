using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Attendance.RemoveAttendance;
using OfficeDays.Features.Attendance.RemoveAttendance.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Attendance.RemoveAttendance;

public sealed class RemoveAttendanceEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new RemoveAttendanceEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<RemoveAttendanceRequest>(epb, executor, "{date}", HttpMethod.Delete);
    }
}
