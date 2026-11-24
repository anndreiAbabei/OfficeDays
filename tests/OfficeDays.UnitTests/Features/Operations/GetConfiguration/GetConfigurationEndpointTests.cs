using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Operations.GetConfiguration.Contracts;
using OfficeDays.Features.Operations.GetConfiguration;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Operations.GetConfiguration;

public sealed class GetConfigurationEndpointTests
{
    [Fact]
    public void Should_MapGetConfiguration_WhenMappingEndpoint()
    {
        // Arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetConfigurationEndpoint(executor);

        // Act
        sut.MapEndpoint(epb);

        // Assert
        Assert.Endpoint<GetConfigurationRequest>(epb, executor, "configuration", HttpMethod.Get);
    }
}
