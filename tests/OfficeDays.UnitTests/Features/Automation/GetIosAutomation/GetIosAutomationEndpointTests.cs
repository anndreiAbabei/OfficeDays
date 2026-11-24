using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Automation.GetIosAutomation.Contracts;
using OfficeDays.Features.Automation.GetIosAutomation;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Automation.GetIosAutomation;

public sealed class GetIosAutomationEndpointTests
{
    [Fact]
    public void Should_MapGetRequest_WhenRegisteringEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetIosAutomationEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<GetIosAutomationRequest>(epb, executor, "ios", HttpMethod.Get);
    }
}
