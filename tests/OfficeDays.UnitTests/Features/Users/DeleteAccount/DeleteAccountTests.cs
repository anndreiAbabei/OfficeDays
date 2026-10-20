using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OfficeDays.Features.Users.DeleteAccount;
using OfficeDays.Features.Users.DeleteAccount.Contracts;
using OfficeDays.UnitTests.Testing;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Users.DeleteAccount;

public sealed class DeleteAccountTests
{
    [Fact]
    public void Should_RequireBrowserAuthorization_WhenMappingEndpoint()
    {
        // Arrange
        var executor = new RequestExecutor();
        var builder = Substitute.For<IEndpointRouteBuilder>();
        builder.DataSources.Returns([]);
        var endpoint = new DeleteAccountEndpoint(executor);

        // Act
        endpoint.MapEndpoint(builder);

        // Assert
        Assert.Endpoint<DeleteAccountRequest>(builder, executor, "me", HttpMethod.Delete);
        var mapped = Assert.Single(builder.DataSources.SelectMany(source => source.Endpoints));
        const string expectedPolicy = "BrowserOnly";
        Assert.Equal(expectedPolicy, mapped.Metadata.GetMetadata<IAuthorizeData>()?.Policy);
    }

    [Theory]
    [InlineData("password", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Should_ValidatePassword_WhenDeletingAccount(string? password, bool expectedValid)
    {
        // Arrange
        // Explicit JSON null can reach validation despite the non-nullable contract.
        var body = new DeleteAccountRequestBody { Password = password! };
        var request = new DeleteAccountRequest { Body = body };
        var validator = new DeleteAccountRequestValidator(new DeleteAccountRequestBodyValidator());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        Assert.Same(body, request.Body);
        Assert.Equal(password, body.Password);
        Assert.Equal(body, body with { });
    }

    [Fact]
    public void Should_RejectMissingBody_WhenDeletingAccount()
    {
        // Arrange: model binding may supply null for an explicitly null JSON body.
        var request = new DeleteAccountRequest { Body = null! };
        var validator = new DeleteAccountRequestValidator(new DeleteAccountRequestBodyValidator());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_LogRejectedDeletionReasons_WhenCalled()
    {
        // Arrange
        var logger = new FakeLogger<DeleteAccountHandler>();
        var expectedUserId = Guid.NewGuid();

        // Act
        logger.LogUserNotFound(expectedUserId);
        logger.LogPasswordRejected(expectedUserId);
        logger.LogLastAdministrator(expectedUserId);

        // Assert
        int[] expectedEventIds = [4601, 4602, 4603];
        Assert.Equal(expectedEventIds, logger.Logs.Select(log => log.EventId.Id));
        Assert.All(logger.Logs, log => Assert.Equal(expectedUserId, log.Values["UserId"]));
        logger.Assert(LogLevel.Warning, $"User {expectedUserId} was not found for account deletion");
        logger.Assert(LogLevel.Warning, $"Account deletion password rejected for user {expectedUserId}");
        logger.Assert(LogLevel.Warning, $"Account deletion blocked for last administrator {expectedUserId}");
    }
}
