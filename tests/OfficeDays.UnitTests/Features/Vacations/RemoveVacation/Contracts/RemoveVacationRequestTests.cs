using OfficeDays.Features.Vacations.RemoveVacation.Contracts;

namespace OfficeDays.UnitTests.Features.Vacations.RemoveVacation.Contracts;

public sealed class RemoveVacationRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();

        // act
        var sut = new RemoveVacationRequest
        {
            Id = expectedId
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
    }
}
