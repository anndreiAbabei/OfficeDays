using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Vacations.CreateVacation.Contracts;
using OfficeDays.Features.Vacations.CreateVacation;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Vacations.CreateVacation;

public sealed class CreateVacationEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new CreateVacationEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<CreateVacationRequest>(epb, executor, "/", HttpMethod.Post);
    }
}
