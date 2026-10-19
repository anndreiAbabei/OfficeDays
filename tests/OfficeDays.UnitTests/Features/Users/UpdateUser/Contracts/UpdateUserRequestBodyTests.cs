using System.Text.Json;
using OfficeDays.Features.Users.UpdateUser.Contracts;

namespace OfficeDays.UnitTests.Features.Users.UpdateUser.Contracts;

public sealed class UpdateUserRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedEmail = "user@example.com";

        // act
        var sut = new UpdateUserRequestBody(expectedEmail);

        // assert
        Assert.Equal(expectedEmail, sut.Email);
    }

    [Fact]
    public void Should_LeaveTeamDayUnspecified_WhenOmitted()
    {
        // arrange
        var expectedEmail = "alice@example.com";

        // act
        var sut = new UpdateUserRequestBody(expectedEmail);

        // assert
        Assert.Equal(expectedEmail, sut.Email);
        Assert.Null(sut.TeamDay);
        Assert.False(sut.HasTeamDay);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("monday")]
    public void Should_MarkTeamDaySpecified_WhenAssigned(string? teamDay)
    {
        // arrange
        var expectedTeamDay = teamDay;

        // act
        var sut = new UpdateUserRequestBody(null) { TeamDay = expectedTeamDay };

        // assert
        Assert.Equal(expectedTeamDay, sut.TeamDay);
        Assert.True(sut.HasTeamDay);
    }

    [Fact]
    public void Should_DistinguishMissingAndExplicitNullTeamDay()
    {
        // arrange
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // act
        var omitted = JsonSerializer.Deserialize<UpdateUserRequestBody>("{}", options)!;
        var explicitNull = JsonSerializer.Deserialize<UpdateUserRequestBody>("{\"teamDay\":null}", options)!;

        // assert
        Assert.False(omitted.HasTeamDay);
        Assert.True(explicitNull.HasTeamDay);
        Assert.Null(explicitNull.TeamDay);
    }

    [Fact]
    public void Should_ExcludeHasTeamDayFromJson()
    {
        // arrange
        var sut = new UpdateUserRequestBody(null) { TeamDay = "monday" };

        // act
        var result = JsonSerializer.SerializeToElement(sut);

        // assert
        Assert.False(result.TryGetProperty("HasTeamDay", out _));
        Assert.Equal("monday", result.GetProperty("TeamDay").GetString());
    }
}
