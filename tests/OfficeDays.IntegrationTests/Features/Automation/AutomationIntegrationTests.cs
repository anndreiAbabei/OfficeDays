using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Automation;

public sealed class AutomationIntegrationTests
{
    [Theory]
    [InlineData("https://www.icloud.com/shortcuts/example", "https://www.icloud.com/shortcuts/example")]
    [InlineData("", null)]
    [InlineData("https://www.icloud.com.example.org/shortcuts/example", null)]
    [InlineData("http://www.icloud.com/shortcuts/example", null)]
    public async Task Should_ReturnUsableLink_WhenUserReadsSetup(string configuredUrl, string? expectedUrl)
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        await using var configured = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(
            (_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Automation:Ios:ShortcutUrl"] = configuredUrl
            })));
        using var browser = configured.CreateClient();
        await CreateUser(browser, "automationuser");
        await Login(browser, "automationuser", "Normal-user-password!");

        // Act
        var result = await browser.GetFromJsonAsync<JsonElement>("/api/automation/ios", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedUrl, result.GetProperty("shortcutUrl").GetString());
    }

    [Fact]
    public async Task Should_RejectRequest_WhenAnonymous()
    {
        // Arrange
        const HttpStatusCode expectedStatus = HttpStatusCode.Unauthorized;
        await using var factory = new OfficeDaysFactory();
        using var browser = factory.CreateClient();

        // Act
        using var result = await browser.GetAsync("/api/automation/ios", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, result.StatusCode);
    }
}
