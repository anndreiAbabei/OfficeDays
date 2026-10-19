using OfficeDays.Features.Vacations.UpdateVacation.Contracts;

namespace OfficeDays.UnitTests.Features.Vacations.UpdateVacation.Contracts;

public sealed class UpdateVacationRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedBody = new UpdateVacationRequestBody(DateOnly.FromDateTime(DateTime.Now), new DateOnly(2026, 10, 2));

        // act
        var sut = new UpdateVacationRequest
        {
            Id = expectedId,
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedBody, sut.Body);
    }
}
