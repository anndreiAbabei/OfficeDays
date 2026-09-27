using System.Diagnostics.CodeAnalysis;

namespace OfficeDays.Infrastructure;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder endpoints);
}
public interface IEndpointGroup
{
    void MapGroup(IEndpointGroupBuilder endpoints);
}

public interface IRootEndpointGroup : IEndpointGroup;

public interface IEndpointGroupBuilder
{
    RouteGroupBuilder MapEndpoints<T>([StringSyntax("Route")] string prefix)
        where T : IEndpoint;
}

public sealed class EndpointGroupBuilder : IEndpointGroupBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EndpointMappingRegistry _mappings;
    private readonly RouteGroupBuilder _routeGroupBuilder;

    public EndpointGroupBuilder(IServiceProvider serviceProvider, RouteGroupBuilder routeGroupBuilder, EndpointMappingRegistry mappings)
    {
        _serviceProvider = serviceProvider;
        _mappings = mappings;
        _routeGroupBuilder = routeGroupBuilder;
    }

    public RouteGroupBuilder MapEndpoints<T>([StringSyntax("Route")] string prefix)
        where T : IEndpoint
    {
        var group = _routeGroupBuilder.MapGroup(prefix);
        var endpoints = _serviceProvider.GetServices<T>().ToArray();
        if (endpoints.Length == 0)
            throw new InvalidOperationException($"Endpoint group interface '{typeof(T).FullName}' has no registered endpoints.");

        foreach (var endpoint in endpoints)
        {
            _mappings.Record(endpoint.GetType());
            endpoint.MapEndpoint(group);
        }

        return group;
    }
}
