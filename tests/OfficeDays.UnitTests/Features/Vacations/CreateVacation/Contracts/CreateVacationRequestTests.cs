using OfficeDays.Features.Vacations.CreateVacation.Contracts;

namespace OfficeDays.UnitTests.Features.Vacations.CreateVacation.Contracts;

public sealed class CreateVacationRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedBody = new CreateVacationRequestBody(DateOnly.FromDateTime(DateTime.Now), new DateOnly(2026, 10, 2));

        // act
        var sut = new CreateVacationRequest
        {
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedBody, sut.Body);
    }
}
