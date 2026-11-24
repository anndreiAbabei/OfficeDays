using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace OfficeDays.IntegrationTests.Features.Operations;

public sealed class ConfigurationIntegrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_ExposeOnlyPublishedNotice_WhenAnonymousClientReadsConfiguration(bool enabled)
    {
        // Arrange
        const string notice = "Our public privacy notice.";
        const string expectedName = "Test operator";
        const string expectedEmail = "privacy@example.org";
        const double expectedLifetime = 86400;
        const int expectedFieldCount = 2;
        await using var factory = new OfficeDaysFactory();
        using var host = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Privacy:Enabled"] = enabled.ToString(),
                ["Privacy:OperatorName"] = expectedName,
                ["Privacy:ContactEmail"] = expectedEmail,
                ["Privacy:OperatorNotice"] = notice
            }));
            builder.ConfigureServices(services => services.PostConfigure<CookieAuthenticationOptions>(
                CookieAuthenticationDefaults.AuthenticationScheme, options => options.ExpireTimeSpan = TimeSpan.FromSeconds(expectedLifetime)));
        });
        using var client = host.CreateClient();

        // Act
        using var response = await client.GetAsync("/api/configuration", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(expectedFieldCount, json.RootElement.EnumerateObject().Count());
        Assert.Equal(expectedLifetime, json.RootElement.GetProperty("persistentLoginSeconds").GetDouble());
        var published = json.RootElement.GetProperty("privacyNotice");
        if (enabled)
        {
            Assert.Equal(expectedName, published.GetProperty("operatorName").GetString());
            Assert.Equal(expectedEmail, published.GetProperty("contactEmail").GetString());
            Assert.Equal(notice, published.GetProperty("details").GetString());
        }
        else
        {
            Assert.Equal(JsonValueKind.Null, published.ValueKind);
        }
    }

    [Theory]
    [InlineData("OperatorNotice", " ")]
    [InlineData("OperatorName", " ")]
    [InlineData("ContactEmail", "invalid")]
    public async Task Should_RejectIncompleteNotice_WhenPublishingIsEnabled(string setting, string value)
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Privacy:Enabled"] = "true",
                [$"Privacy:{setting}"] = value
            })));

        // Act
        var error = Record.Exception(() => { using var client = host.CreateClient(); });

        // Assert
        Assert.IsType<OptionsValidationException>(error);
    }
}
