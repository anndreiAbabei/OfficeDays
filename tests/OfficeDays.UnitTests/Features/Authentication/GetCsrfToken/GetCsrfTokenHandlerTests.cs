using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using OfficeDays.Features.Authentication.GetCsrfToken;
using OfficeDays.Features.Authentication.GetCsrfToken.Contracts;

namespace OfficeDays.UnitTests.Features.Authentication.GetCsrfToken;

public sealed class GetCsrfTokenHandlerTests
{
    [Theory]
    [InlineData("request-token")]
    [InlineData(null)]
    public async Task Should_ReturnRequestToken_WhenAntiforgeryTokensAreCreated(string? expectedToken)
    {
        // Arrange
        const int expectedStatusCode = StatusCodes.Status200OK;
        var context = new DefaultHttpContext();
        var antiforgery = Substitute.For<IAntiforgery>();
        var tokens = new AntiforgeryTokenSet(expectedToken, "cookie-token", "__RequestVerificationToken", "X-CSRF-TOKEN");
        antiforgery.GetAndStoreTokens(context).Returns(tokens);
        var contextAccessor = new HttpContextAccessor { HttpContext = context };
        var sut = new GetCsrfTokenHandler(antiforgery, contextAccessor);

        // Act
        var result = await sut.Handle(new GetCsrfTokenRequest(), TestContext.Current.CancellationToken);

        // Assert
        var response = Assert.IsType<Ok<GetCsrfTokenResponse>>(result);
        var body = Assert.IsType<GetCsrfTokenResponse>(response.Value);
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(expectedToken, body.Token);
        antiforgery.Received(1).GetAndStoreTokens(context);
    }

    [Fact]
    public async Task Should_RejectRequestWithoutCreatingTokens_WhenHttpContextIsMissing()
    {
        // Arrange
        const string expectedMessage = "No active HTTP request.";
        var antiforgery = Substitute.For<IAntiforgery>();
        var sut = new GetCsrfTokenHandler(antiforgery, new HttpContextAccessor());

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Handle(new GetCsrfTokenRequest(), TestContext.Current.CancellationToken).AsTask());

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
        antiforgery.DidNotReceive().GetAndStoreTokens(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task Should_StopWithoutCreatingTokens_WhenRequestIsCancelled()
    {
        // Arrange
        var cancellationToken = new CancellationToken(canceled: true);
        var antiforgery = Substitute.For<IAntiforgery>();
        var contextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var sut = new GetCsrfTokenHandler(antiforgery, contextAccessor);

        // Act
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() =>
            sut.Handle(new GetCsrfTokenRequest(), cancellationToken).AsTask());

        // Assert
        Assert.Equal(cancellationToken, exception.CancellationToken);
        antiforgery.DidNotReceive().GetAndStoreTokens(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task Should_PropagateFailure_WhenAntiforgeryCannotCreateTokens()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Token creation failed.");
        var context = new DefaultHttpContext();
        var antiforgery = Substitute.For<IAntiforgery>();
        antiforgery.GetAndStoreTokens(context).Returns(_ => throw expectedException);
        var contextAccessor = new HttpContextAccessor { HttpContext = context };
        var sut = new GetCsrfTokenHandler(antiforgery, contextAccessor);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Handle(new GetCsrfTokenRequest(), TestContext.Current.CancellationToken).AsTask());

        // Assert
        Assert.Same(expectedException, exception);
        antiforgery.Received(1).GetAndStoreTokens(context);
    }
}
