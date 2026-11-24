using OfficeDays.Features.Automation.GetIosAutomation.Contracts;

namespace OfficeDays.UnitTests.Features.Automation.GetIosAutomation.Contracts;

public sealed class GetIosAutomationResponseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("https://www.icloud.com/shortcuts/example")]
    public void Should_ExposeConfiguredLink_WhenResponseIsCreated(string? expectedUrl)
    {
        // Arrange and act
        var response = new GetIosAutomationResponse(expectedUrl);

        // Assert
        Assert.Equal(expectedUrl, response.ShortcutUrl);
    }
}
