using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Vacations.RemoveVacation.Contracts;
using OfficeDays.Features.Vacations.RemoveVacation;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Vacations.RemoveVacation;

public sealed class RemoveVacationEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new RemoveVacationEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<RemoveVacationRequest>(epb, executor, "{id:guid}", HttpMethod.Delete);
    }
}
