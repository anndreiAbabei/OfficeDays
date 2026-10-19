using OfficeDays.Features.Users.GetRequirements.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Users.GetRequirements.Contracts;

public sealed class GetRequirementsRequestTests
{
    [Fact]
    public void Should_CreateRequest()
    {
        // arrange
        // No input is required.

        // act
        var sut = new GetRequirementsRequest();

        // assert
        Assert.IsAssignableFrom<IRequest>(sut);
    }
}
