using FluentValidation;
using OfficeDays.Infrastructure;

namespace OfficeDays.Configuration;

public static class FeatureRegistration
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        var assembly = typeof(Program).Assembly;
        var catalog = FeatureCatalog.Discover(assembly.GetExportedTypes());
        foreach (var group in catalog.Groups)
            services.AddSingleton(typeof(IEndpointGroup), group);
        foreach (var endpoint in catalog.Endpoints)
            services.AddSingleton(endpoint.GroupInterface, endpoint.EndpointType);
        foreach (var handler in catalog.Handlers)
            services.AddScoped(typeof(IRequestHandler<>).MakeGenericType(handler.RequestType), handler.HandlerType);

        services.AddValidatorsFromAssembly(assembly);
        services.AddSingleton(_ =>
        {
            // Check the final collection, including replacements made by a host or test fixture.
            catalog.ValidateServices(services);
            return catalog;
        });
        return services;
    }
}
