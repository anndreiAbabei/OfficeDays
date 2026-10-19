using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Vacations.GetVacations.Contracts;
using OfficeDays.Features.Vacations.GetVacations;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Vacations.GetVacations;

public sealed class GetVacationsEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetVacationsEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<GetVacationsRequest>(epb, executor, "/", HttpMethod.Get);
    }
}
