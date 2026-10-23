using OfficeDays.Domain;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.UnitTests.Features.Users.UpdateRequirement.Contracts;

public sealed class UpdateRequirementRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedBody = new UpdateRequirementRequestBody { RequirementMode = AttendanceRequirementMode.Percentage, StartingFrom = DateOnly.FromDateTime(DateTime.Now), RequirementValue = 22 };

        // act
        var sut = new UpdateRequirementRequest
        {
            Id = expectedId,
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedBody, sut.Body);
    }
}
