using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateUser.Contracts;

namespace OfficeDays.UnitTests.Features.Users.CreateUser.Contracts;

public sealed class CreateUserRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedBody = new CreateUserRequestBody { RequirementMode = AttendanceRequirementMode.Percentage, Username = "Username value", Password = "a-long-password", TimeZoneId = "Europe/Bucharest", CountryCode = "RO", Email = "user@example.com", RequirementValue = 26 };

        // act
        var sut = new CreateUserRequest
        {
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedBody, sut.Body);
    }
}
