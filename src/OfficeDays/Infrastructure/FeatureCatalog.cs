namespace OfficeDays.Infrastructure;

public sealed record EndpointRegistration(Type EndpointType, Type GroupInterface);
public sealed record HandlerRegistration(Type RequestType, Type HandlerType);

/// <summary>The discovered feature types and the registrations required to execute them.</summary>
public sealed class FeatureCatalog
{
    public IReadOnlyList<Type> Groups { get; }
    public IReadOnlyList<EndpointRegistration> Endpoints { get; }
    public IReadOnlyList<HandlerRegistration> Handlers { get; }

    private FeatureCatalog(Type[] groups, EndpointRegistration[] endpoints, HandlerRegistration[] handlers)
    {
        Groups = Array.AsReadOnly(groups);
        Endpoints = Array.AsReadOnly(endpoints);
        Handlers = Array.AsReadOnly(handlers);
    }

    public static FeatureCatalog Discover(IEnumerable<Type> types)
    {
        var concreteTypes = types.Distinct()
            .Where(type => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters)
            .OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();
        var groups = concreteTypes.Where(type => typeof(IEndpointGroup).IsAssignableFrom(type)).ToArray();
        var endpoints = concreteTypes.Where(type => typeof(IEndpoint).IsAssignableFrom(type)).Select(type =>
        {
            var markers = type.GetInterfaces()
                .Where(contract => contract != typeof(IEndpoint) && typeof(IEndpoint).IsAssignableFrom(contract))
                .ToArray();
            // An inherited marker counts as part of its most specific marker, not as another group.
            var leafMarkers = markers.Where(marker => !markers.Any(other => other != marker && marker.IsAssignableFrom(other))).ToArray();
            if (leafMarkers.Length != 1)
                throw new InvalidOperationException($"Endpoint '{type.FullName}' must implement exactly one feature endpoint group interface; found {leafMarkers.Length}.");
            return new EndpointRegistration(type, leafMarkers[0]);
        }).ToArray();

        var handlers = concreteTypes.SelectMany(type => type.GetInterfaces()
            .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequestHandler<>))
            .Select(contract => new HandlerRegistration(contract.GenericTypeArguments[0], type))).ToArray();
        var requests = concreteTypes.Where(type => typeof(IRequest).IsAssignableFrom(type))
            .Concat(handlers.Select(handler => handler.RequestType)).Distinct();
        foreach (var request in requests)
        {
            var matches = handlers.Where(handler => handler.RequestType == request).ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException($"Request '{request.FullName}' must have exactly one handler; found {matches.Length}: {string.Join(", ", matches.Select(match => match.HandlerType.FullName))}.");
        }

        return new FeatureCatalog(groups, endpoints, handlers);
    }

    public void ValidateServices(IServiceCollection services)
    {
        foreach (var handler in Handlers)
        {
            var contract = typeof(IRequestHandler<>).MakeGenericType(handler.RequestType);
            var count = services.Count(descriptor => !descriptor.IsKeyedService && descriptor.ServiceType == contract);
            if (count != 1)
                throw new InvalidOperationException($"Request '{handler.RequestType.FullName}' requires exactly one registered handler; found {count}.");
        }
    }
}
