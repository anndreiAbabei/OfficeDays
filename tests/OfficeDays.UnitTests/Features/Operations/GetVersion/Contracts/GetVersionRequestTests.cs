using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Operations.GetVersion.Contracts;

public sealed class GetVersionRequestTests
{
    [Fact]
    public void Should_CreateRequest()
    {
        // arrange
        // No input is required.

        // act
        var sut = new GetVersionRequest();

        // assert
        Assert.IsAssignableFrom<IRequest>(sut);
    }
}
