namespace OfficeDays.Infrastructure;

/// <summary>Tracks actual group mapping calls so discovered endpoints cannot silently go unused.</summary>
public sealed class EndpointMappingRegistry(FeatureCatalog catalog)
{
    private readonly HashSet<Type> _mapped = [];

    public void Record(Type endpointType)
    {
        if (!catalog.Endpoints.Any(endpoint => endpoint.EndpointType == endpointType))
            throw new InvalidOperationException($"Endpoint '{endpointType.FullName}' was mapped but was not discovered.");
        if (!_mapped.Add(endpointType))
            throw new InvalidOperationException($"Endpoint '{endpointType.FullName}' was mapped more than once. Check its endpoint groups.");
    }

    public void ValidateComplete()
    {
        var missing = catalog.Endpoints.Where(endpoint => !_mapped.Contains(endpoint.EndpointType)).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException($"Discovered endpoints were not mapped by any group: {string.Join(", ", missing.Select(endpoint => endpoint.EndpointType.FullName))}.");
    }
}
