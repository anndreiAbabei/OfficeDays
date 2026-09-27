using OfficeDays.Infrastructure;

namespace OfficeDays.Endpoints;

public static class ApiEndpointMappings
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var catalog = app.Services.GetRequiredService<FeatureCatalog>();
        var mappings = new EndpointMappingRegistry(catalog);
        var api = app.MapGroup("/api");

        IEndpointGroupBuilder groupBuilder = new EndpointGroupBuilder(app.Services, api, mappings);
        foreach (var group in app.Services.GetServices<IEndpointGroup>())
        {
            group.MapGroup(group is IRootEndpointGroup
                ? new EndpointGroupBuilder(app.Services, app.MapGroup(""), mappings)
                : groupBuilder);
        }
        mappings.ValidateComplete();
    }
}
