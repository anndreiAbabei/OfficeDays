using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using OfficeDays.Extensions;
using OfficeDays.Middleware;

namespace OfficeDays.UnitTests.Extensions;

public sealed class MiddlewareExtensionsUseBearerTokenHttpsProtectionTests
{
    [Fact]
    public void Should_AddBearerTokenHttpsMiddlewareMiddleware()
    {
        // arrange
        var app = Substitute.For<IApplicationBuilder>();
        
        // act
        _ = app.UseBearerTokenHttpsProtection();
        
        // assert
        app.Received().Use(Arg.Is<Func<RequestDelegate, RequestDelegate>>(f => f.Target!.ToString()!.Contains(nameof(BearerTokenHttpsMiddleware))));
    }

    [Fact]
    public void Should_ReturnSameInstance()
    {
        // arrange
        var app = Substitute.For<IApplicationBuilder>();
        app.Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>())
           .Returns(app);
        
        // act
        var result = app.UseBearerTokenHttpsProtection();

        // assert
        Assert.Same(result, app);
    }
}
