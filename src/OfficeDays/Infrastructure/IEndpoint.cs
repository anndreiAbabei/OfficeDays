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

public interface IEndpointGroupBuilder
{
    RouteGroupBuilder MapEndpoints<T>([StringSyntax("Route")] string prefix) 
        where T : IEndpoint;
}

public sealed class EndpointGroupBuilder : IEndpointGroupBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RouteGroupBuilder _routeGroupBuilder;

    public EndpointGroupBuilder(IServiceProvider serviceProvider, RouteGroupBuilder routeGroupBuilder)
    {
        _serviceProvider = serviceProvider;
        _routeGroupBuilder = routeGroupBuilder;
    }

    public RouteGroupBuilder MapEndpoints<T>([StringSyntax("Route")] string prefix) 
        where T : IEndpoint
    {
        var group = _routeGroupBuilder.MapGroup(prefix);
        var endpoints = _serviceProvider.GetServices<T>();
        
        foreach (var endpoint in endpoints)
            endpoint.MapEndpoint(group);
        
        return group;
    }
}