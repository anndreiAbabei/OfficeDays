using OfficeDays.Features.Operations.GetConfiguration.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Operations.GetConfiguration.Contracts;

public sealed class GetConfigurationRequestTests
{
    [Fact]
    public void Should_ImplementRequest_WhenCreated()
    {
        // Arrange
        // No input is required.

        // Act
        var sut = new GetConfigurationRequest();

        // Assert
        Assert.IsAssignableFrom<IRequest>(sut);
    }
}
