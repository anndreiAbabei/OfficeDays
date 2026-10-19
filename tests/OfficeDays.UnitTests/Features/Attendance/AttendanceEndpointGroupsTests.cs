using Microsoft.AspNetCore.Builder;
using NSubstitute;
using OfficeDays.Features.Attendance;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Attendance;

public sealed class AttendanceEndpointGroupsTests
{
    [Fact]
    public  async Task Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        ep.MapEndpoints<IAttendanceEndpoint>("attendance").Returns(app.MapGroup(""));
        var sut = new AttendanceEndpointGroups();

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received().MapEndpoints<IAttendanceEndpoint>("attendance");
    }
}
