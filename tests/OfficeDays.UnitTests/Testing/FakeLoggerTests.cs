using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Xunit.Sdk;

namespace OfficeDays.UnitTests.Testing;

[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Is for testing")]
[SuppressMessage("ReSharper", "StringLiteralAsInterpolationArgument", Justification = "That's the subject of the test")]
public sealed class FakeLoggerTests
{
    [Fact]
    public void Matches_template_and_all_typed_arguments()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        var userId = Guid.NewGuid();
        logger.LogInformation("User {UserId} logged in the system {Count} times", userId, 3);

        logger.Assert(LogLevel.Information, $"User {userId} logged in the system {3} times");
        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Information, $"User {userId} logged in the system {4} times"));
        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Information, $"User {userId} logged out of the system {3} times"));
        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Information, $"User {userId} logged in the system {"3"} times"));
        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Information, $"User {userId} logged in the system 3 times"));
        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Warning, $"User {userId} logged in the system {3} times"));
    }

    [Fact]
    public void Matches_null_arguments_and_requires_the_same_exception()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        var exception = new InvalidOperationException("Failed");
        string? user = null;
        logger.LogError(exception, "Failed for {User}", user);

        logger.Assert(LogLevel.Error, $"Failed for {user}", exception);
        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Error, $"Failed for {user}"));
        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Error, $"Failed for {user}", new InvalidOperationException("Failed")));
    }

    [Fact]
    public void Preserves_escaped_braces_alignment_and_format_specifiers()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        logger.LogInformation("Value {{{Value,8:N2}}}", 12.5m);

        logger.Assert(LogLevel.Information, $"Value {{{12.5m,8:N2}}}");
        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Information, $"Value {{{12.5m:N2}}}"));
    }

    [Fact]
    public void Matches_messages_without_arguments_and_searches_all_entries()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        logger.LogInformation("Unrelated {Value}", 1);
        logger.LogInformation("Finished");

        logger.Assert(LogLevel.Information, $"Finished");
        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Information, $"Missing"));
    }

    [Fact]
    public void Rejects_rendered_text_without_the_expected_template()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        logger.LogInformation("User 123 logged in");

        Assert.ThrowsAny<XunitException>(() => logger.Assert(LogLevel.Information, $"User {123} logged in"));
    }
}
