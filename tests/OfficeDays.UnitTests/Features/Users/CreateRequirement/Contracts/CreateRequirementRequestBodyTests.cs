using OfficeDays.Features.Users.CreateRequirement.Contracts;

namespace OfficeDays.UnitTests.Features.Users.CreateRequirement.Contracts;

public sealed class CreateRequirementRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedStartingFrom = DateOnly.FromDateTime(DateTime.Now);
        const int expectedRequiredOfficePercentage = 22;

        // act
        var sut = new CreateRequirementRequestBody
        {
            StartingFrom = expectedStartingFrom,
            RequiredOfficePercentage = expectedRequiredOfficePercentage
        };

        // assert
        Assert.Equal(expectedStartingFrom, sut.StartingFrom);
        Assert.Equal(expectedRequiredOfficePercentage, sut.RequiredOfficePercentage);
    }
}
