using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.UnitTests.Features.Users.UpdateRequirement.Contracts;

public sealed class UpdateRequirementRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedStartingFrom = DateOnly.FromDateTime(DateTime.Now);
        var expectedRequiredOfficePercentage = 22;

        // act
        var sut = new UpdateRequirementRequestBody
        {
            StartingFrom = expectedStartingFrom,
            RequiredOfficePercentage = expectedRequiredOfficePercentage
        };

        // assert
        Assert.Equal(expectedStartingFrom, sut.StartingFrom);
        Assert.Equal(expectedRequiredOfficePercentage, sut.RequiredOfficePercentage);
    }
}
