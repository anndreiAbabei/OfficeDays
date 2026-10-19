using OfficeDays.Features.Users.DeleteRequirement.Contracts;

namespace OfficeDays.UnitTests.Features.Users.DeleteRequirement.Contracts;

public sealed class DeleteRequirementRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();

        // act
        var sut = new DeleteRequirementRequest
        {
            Id = expectedId
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
    }
}
