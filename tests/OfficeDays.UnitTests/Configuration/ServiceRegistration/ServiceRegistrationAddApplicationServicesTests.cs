using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration.Memory;
using OfficeDays.Configuration;

namespace OfficeDays.UnitTests.Configuration;

public sealed class ServiceRegistrationAddApplicationServicesTests
{
    private readonly WebApplicationBuilder _builder;

    public ServiceRegistrationAddApplicationServicesTests()
    {
        var options = new WebApplicationOptions();
        var config = new MemoryConfigurationSource
        {
            InitialData = [new KeyValuePair<string, string?>("Authentication:CookieExpireTimeSpan", "00:00:15")]
        };
        
        _builder = WebApplication.CreateEmptyBuilder(options);
        _builder.Configuration.Sources.Add(config);
    }
    
    [Fact]
    public void Should_ReturnSameBuilder()
    {
        // arrange
        // act
        var result = _builder.AddApplicationServices();

        // assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_WhenCookieExpireTimeSpanIsNotSet()
    {
        // arrange
        var options = new WebApplicationOptions();
        var builder = WebApplication.CreateEmptyBuilder(options);
        const string expectedExceptionMessage = "Authentication:CookieExpireTimeSpan must be a positive TimeSpan.";
        
        // act
        Action action = () => builder.AddApplicationServices();

        // assert
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(expectedExceptionMessage, exception.Message);
    }
}
