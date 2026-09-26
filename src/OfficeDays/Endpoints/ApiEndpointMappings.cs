using OfficeDays.Infrastructure;

namespace OfficeDays.Endpoints;

public static class ApiEndpointMappings
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        IEndpointGroupBuilder groupBuilder = new EndpointGroupBuilder(app.Services, api);
        foreach (var group in app.Services.GetServices<IEndpointGroup>())
        {
            group.MapGroup(group is IRootEndpointGroup
                ? new EndpointGroupBuilder(app.Services, app.MapGroup(""))
                : groupBuilder);
        }
    }
}
