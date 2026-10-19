using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Attendance.RecordDate;
using OfficeDays.Features.Attendance.RecordDate.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Attendance.RecordDate;

public sealed class RecordDateEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new RecordDateEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<RecordDateRequest>(epb, executor, "{date}", HttpMethod.Put);
    }
}
