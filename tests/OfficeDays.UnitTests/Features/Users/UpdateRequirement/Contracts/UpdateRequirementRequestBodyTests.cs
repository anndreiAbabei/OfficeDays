using OfficeDays.Domain;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.UnitTests.Features.Users.UpdateRequirement.Contracts;

public sealed class UpdateRequirementRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedStartingFrom = DateOnly.FromDateTime(DateTime.Now);
        var expectedValue = 22;

        // act
        var sut = new UpdateRequirementRequestBody
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
