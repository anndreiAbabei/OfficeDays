using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Configuration;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Configuration;

public sealed class FeatureRegistrationAddFeaturesTests
{
    [Fact]
    public void Should_ReturnSameServiceCollection()
    {
        // arrange
        var sc = new ServiceCollection();
        
        // act
        var result = sc.AddFeatures();

        // assert
        Assert.Same(sc, result);
    }

    [Fact]
    public void Should_AddEndpointGroups()
    {
        // arrange
        var sc = new ServiceCollection();
        
        // act
        _ = sc.AddFeatures();

        // assert
        Assert.Contains(sc, s => s.ServiceType == typeof(IEndpointGroup));
    }

    [Fact]
    public void Should_AddEndpoints()
    {
        // arrange
        var sc = new ServiceCollection();
        
        // act
        _ = sc.AddFeatures();

        // assert
        Assert.Contains(sc, s => s.ServiceType.IsAssignableTo(typeof(IEndpoint)));
    }

    [Fact]
    public void Should_AddHandlers()
    {
        // arrange
        var sc = new ServiceCollection();
        
        // act
        _ = sc.AddFeatures();

        // assert
        Assert.Contains(sc, s => s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<>));
    }

    [Fact]
    public void Should_AddValidators()
    {
        // arrange
        var sc = new ServiceCollection();
        
        // act
        _ = sc.AddFeatures();

        // assert
        Assert.Contains(sc, s => s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>));
    }

    [Fact]
    public void Should_AddFeatureCatalog()
    {
        // arrange
        var sc = new ServiceCollection();
        
        // act
        _ = sc.AddFeatures();

        // assert
        Assert.Contains(sc, s => s.ServiceType == typeof(FeatureCatalog));
    }
}
