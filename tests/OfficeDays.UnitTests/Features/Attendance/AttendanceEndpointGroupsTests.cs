using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Attendance;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Attendance;

public sealed class AttendanceEndpointGroupsTests
{
    [Fact]
    public void Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        var app = WebApplication.CreateSlimBuilder();
        ep.MapEndpoints<IAttendanceEndpoint>("attendance").Returns(app.Build().MapGroup(""));
        var sut = new AttendanceEndpointGroups();

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received().MapEndpoints<IAttendanceEndpoint>("attendance");
    }
}
