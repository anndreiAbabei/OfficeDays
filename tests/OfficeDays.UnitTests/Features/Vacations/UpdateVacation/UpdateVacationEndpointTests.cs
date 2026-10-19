using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Vacations.UpdateVacation.Contracts;
using OfficeDays.Features.Vacations.UpdateVacation;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Vacations.UpdateVacation;

public sealed class UpdateVacationEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new UpdateVacationEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<UpdateVacationRequest>(epb, executor, "{id:guid}", HttpMethod.Put);
    }
}
