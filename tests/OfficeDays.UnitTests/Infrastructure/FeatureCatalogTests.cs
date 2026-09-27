using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Infrastructure;

public sealed class FeatureCatalogTests
{
    [Fact]
    public void Missing_handler_reports_the_request()
    {
        var error = Assert.Throws<InvalidOperationException>(() => FeatureCatalog.Discover([typeof(TestRequest)]));
        Assert.Contains(typeof(TestRequest).FullName!, error.Message);
        Assert.Contains("found 0", error.Message);
    }

    [Fact]
    public void Duplicate_handlers_report_both_implementations()
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            FeatureCatalog.Discover([typeof(TestRequest), typeof(FirstHandler), typeof(SecondHandler)]));
        Assert.Contains(typeof(FirstHandler).FullName!, error.Message);
        Assert.Contains(typeof(SecondHandler).FullName!, error.Message);
    }

    [Fact]
    public void Endpoint_without_a_group_marker_is_rejected()
    {
        var error = Assert.Throws<InvalidOperationException>(() => FeatureCatalog.Discover([typeof(UngroupedEndpoint)]));
        Assert.Contains(typeof(UngroupedEndpoint).FullName!, error.Message);
        Assert.Contains("group interface", error.Message);
    }

    [Fact]
    public void Endpoint_with_ambiguous_groups_is_rejected()
    {
        var error = Assert.Throws<InvalidOperationException>(() => FeatureCatalog.Discover([typeof(AmbiguousEndpoint)]));
        Assert.Contains("found 2", error.Message);
    }

    [Fact]
    public void Every_handler_interface_is_discovered()
    {
        var catalog = FeatureCatalog.Discover([typeof(TestRequest), typeof(OtherRequest), typeof(MultiHandler)]);
        Assert.Equal(2, catalog.Handlers.Count);
        Assert.Contains(catalog.Handlers, handler => handler.RequestType == typeof(TestRequest));
        Assert.Contains(catalog.Handlers, handler => handler.RequestType == typeof(OtherRequest));
    }

    [Fact]
    public void A_discovered_endpoint_must_actually_be_mapped_once()
    {
        var registry = new EndpointMappingRegistry(FeatureCatalog.Discover([typeof(TestEndpoint)]));
        Assert.Contains(typeof(TestEndpoint).FullName!, Assert.Throws<InvalidOperationException>(registry.ValidateComplete).Message);
        registry.Record(typeof(TestEndpoint));
        registry.ValidateComplete();
        Assert.Contains("more than once", Assert.Throws<InvalidOperationException>(() => registry.Record(typeof(TestEndpoint))).Message);
    }

    [Fact]
    public void Final_service_validation_allows_replacement_but_not_multiple_registrations()
    {
        var catalog = FeatureCatalog.Discover([typeof(TestRequest), typeof(FirstHandler)]);
        var services = new ServiceCollection();
        Assert.Throws<InvalidOperationException>(() => catalog.ValidateServices(services));
        services.AddScoped<IRequestHandler<TestRequest>, SecondHandler>();
        catalog.ValidateServices(services);
        services.AddScoped<IRequestHandler<TestRequest>, FirstHandler>();
        Assert.Throws<InvalidOperationException>(() => catalog.ValidateServices(services));
    }

    private sealed record TestRequest : IRequest;
    private sealed record OtherRequest : IRequest;
    private sealed class FirstHandler : IRequestHandler<TestRequest>
    {
        public ValueTask<IResult> Handle(TestRequest request, CancellationToken cancellationToken) => ValueTask.FromResult(Results.Empty);
    }
    private sealed class SecondHandler : IRequestHandler<TestRequest>
    {
        public ValueTask<IResult> Handle(TestRequest request, CancellationToken cancellationToken) => ValueTask.FromResult(Results.Empty);
    }
    private sealed class MultiHandler : IRequestHandler<TestRequest>, IRequestHandler<OtherRequest>
    {
        public ValueTask<IResult> Handle(TestRequest request, CancellationToken cancellationToken) => ValueTask.FromResult(Results.Empty);
        public ValueTask<IResult> Handle(OtherRequest request, CancellationToken cancellationToken) => ValueTask.FromResult(Results.Empty);
    }
    private interface ITestEndpoint : IEndpoint;
    private interface IOtherEndpoint : IEndpoint;
    private sealed class TestEndpoint : ITestEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
    }
    private sealed class UngroupedEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
    }
    private sealed class AmbiguousEndpoint : ITestEndpoint, IOtherEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
    }
}
