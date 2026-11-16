using System.Net;
using System.Text.Json;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Users;

public sealed class AppearanceIntegrationTests
{
    [Theory]
    [InlineData("System")]
    [InlineData("Dark")]
    [InlineData("Light")]
    public async Task Should_PreserveSavedAppearance_WhenUpdateIsInvalidOrOmitsAppearance(string expectedAppearance)
    {
        // Arrange
        const string expectedOtherAppearance = "System";
        const HttpStatusCode expectedRejection = HttpStatusCode.BadRequest;
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "first");
        await CreateUser(client, "second");
        await Login(client, "first", "Normal-user-password!");
        var csrf = await Csrf(client);
        var saved = await PutJson(client, "/api/users/me", new { appearance = expectedAppearance }, csrf);
        saved.EnsureSuccessStatusCode();

        // Act
        var invalid = await PutJson(client, "/api/users/me", new { appearance = "blue" }, csrf);
        var omitted = await PutJson(client, "/api/users/me", new { email = "first@example.com" }, csrf);
        var first = await client.GetFromJsonAsync<JsonElement>("/api/auth/me", TestContext.Current.CancellationToken);
        var export = await client.GetFromJsonAsync<JsonElement>("/api/users/me/export", TestContext.Current.CancellationToken);
        await Login(client, "second", "Normal-user-password!");
        var second = await client.GetFromJsonAsync<JsonElement>("/api/auth/me", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedAppearance, export.GetProperty("preferences").GetProperty("appearance").GetString());
        Assert.Equal(expectedRejection, invalid.StatusCode);
        Assert.Equal(expectedAppearance, (await Read(omitted)).GetProperty("appearance").GetString());
        Assert.Equal(expectedAppearance, first.GetProperty("appearance").GetString());
        Assert.Equal(expectedOtherAppearance, second.GetProperty("appearance").GetString());
    }
}
