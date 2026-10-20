using OfficeDays.Security.RateLimiting;

namespace OfficeDays.UnitTests.Security.RateLimiting;

public sealed class RequestLimitsTests
{
    [Fact]
    public void Should_UseDocumentedDefaults_WhenConstructed()
    {
        // Arrange
        const int expectedBodyBytes = 65_536;
        var expectedLimits = new[]
        {
            new RequestLimit(600, 60),
            new RequestLimit(120, 60),
            new RequestLimit(60, 60),
            new RequestLimit(20, 300),
            new RequestLimit(10, 300),
            new RequestLimit(5, 3600),
            new RequestLimit(10, 900),
            new RequestLimit(5, 900),
            new RequestLimit(10, 900),
            new RequestLimit(10, 60)
        };

        // Act
        var limits = new RequestLimits();

        // Assert
        Assert.Equal(expectedBodyBytes, limits.MaxBodyBytes);
        var limitsList = new[]
        {
            limits.Source, 
            limits.Api,
            limits.Token, 
            limits.Login,
            limits.LoginAccount, 
            limits.Registration, 
            limits.Recovery, 
            limits.RecoveryAccount,
            limits.Verification, 
            limits.Reports
        };
        Assert.Equal(expectedLimits, limitsList);
    }

    [Fact]
    public void Should_PreservePolicyAndOptionalAccountField_WhenMetadataIsConstructed()
    {
        // Arrange
        const string expectedPolicy = "recovery";
        const string expectedField = "email";

        // Act
        var targeted = new SensitiveRequestLimit(expectedPolicy, expectedField);
        var untargeted = new SensitiveRequestLimit("verification");

        // Assert
        Assert.Equal(expectedPolicy, targeted.Policy);
        Assert.Equal(expectedField, targeted.AccountField);
        Assert.Null(untargeted.AccountField);
        Assert.Equal(new SensitiveRequestLimit(expectedPolicy, expectedField), targeted);
    }
}
