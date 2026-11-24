using OfficeDays.Features.Operations.GetConfiguration.Contracts;

namespace OfficeDays.UnitTests.Features.Operations.GetConfiguration.Contracts;

public sealed class GetConfigurationResponseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("Published notice")]
    public void Should_ReturnPublishedText_WhenConstructed(string? expectedNotice)
    {
        // Arrange
        const string expectedName = "Operator";
        const string expectedEmail = "mail@example.org";
        const double expectedLifetime = 7200;
        var notice = expectedNotice is null ? null : new PublishedPrivacyNotice(expectedName, expectedEmail, expectedNotice);
        var expectedResponse = new GetConfigurationResponse(notice, expectedLifetime);

        // Act
        var response = new GetConfigurationResponse(notice, expectedLifetime);

        // Assert
        Assert.Same(notice, response.PrivacyNotice);
        Assert.Equal(expectedLifetime, response.PersistentLoginSeconds);
        if (notice is not null)
        {
            Assert.Equal(expectedNotice, notice.Details);
            Assert.Equal(expectedName, notice.OperatorName);
            Assert.Equal(expectedEmail, notice.ContactEmail);
        }
        Assert.Equal(expectedResponse, response);
    }
}
