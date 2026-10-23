using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateRequirement.Contracts;

namespace OfficeDays.UnitTests.Features.Users.CreateRequirement.Contracts;

public sealed class CreateRequirementRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedBody = new CreateRequirementRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            StartingFrom = DateOnly.FromDateTime(DateTime.Now), 
            RequirementValue = 22
        };

        // act
        var sut = new CreateRequirementRequest
        {
            Body = expectedBody
        };

        // assert
        Assert.Same(expectedBody, sut.Body);
    }
}
