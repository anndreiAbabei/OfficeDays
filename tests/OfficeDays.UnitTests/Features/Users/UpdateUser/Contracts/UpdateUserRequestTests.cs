using OfficeDays.Features.Users.UpdateUser.Contracts;

namespace OfficeDays.UnitTests.Features.Users.UpdateUser.Contracts;

public sealed class UpdateUserRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedBody = new UpdateUserRequestBody("user@example.com");

        // act
        var sut = new UpdateUserRequest
        {
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedBody, sut.Body);
    }
}
