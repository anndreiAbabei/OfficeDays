using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateRequirement.Contracts;

namespace OfficeDays.UnitTests.Features.Users.CreateRequirement.Contracts;

public sealed class CreateRequirementRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedStartingFrom = DateOnly.FromDateTime(DateTime.Now);
        const int expectedValue = 22;

        // act
        var sut = new CreateRequirementRequestBody
        {
            RequirementMode = AttendanceRequirementMode.Percentage,
            StartingFrom = expectedStartingFrom,
            RequirementValue = expectedValue
        };

        // assert
        Assert.Equal(expectedStartingFrom, sut.StartingFrom);
        Assert.Equal(expectedValue, sut.RequirementValue);
    }
}
