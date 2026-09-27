using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OfficeDays.Features.Attendance;
using OfficeDays.Features.Operations.GetVersion;
using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.IntegrationTests.Infrastructure;

public sealed class FeatureRegistrationIntegrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Startup_rejects_missing_or_duplicate_handler_registrations(bool duplicate)
    {
        using var original = new OfficeDaysFactory();
        using var factory = original.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            if (duplicate)
                services.AddScoped<IRequestHandler<GetVersionRequest>, GetVersionHandler>();
            else
                services.RemoveAll<IRequestHandler<GetVersionRequest>>();
        }));
        var error = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        Assert.Contains(nameof(GetVersionRequest), error.ToString());
        Assert.Contains("exactly one registered handler", error.ToString());
    }

    [Fact]
    public void Startup_rejects_endpoints_without_a_mapped_group()
    {
        using var original = new OfficeDaysFactory();
        using var factory = original.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            var attendance = services.Single(descriptor => descriptor.ImplementationType == typeof(AttendanceEndpointGroups));
            services.Remove(attendance);
        }));
        var error = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        Assert.Contains("not mapped by any group", error.ToString());
        Assert.Contains("GetAttendanceEndpoint", error.ToString());
    }

    [Fact]
    public void Startup_rejects_groups_mapping_the_same_endpoints_twice()
    {
        using var original = new OfficeDaysFactory();
        using var factory = original.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IEndpointGroup, AttendanceEndpointGroups>()));
        var error = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        Assert.Contains("mapped more than once", error.ToString());
    }

    [Fact]
    public void Startup_rejects_scoped_dependencies_in_singleton_endpoints()
    {
        using var original = new OfficeDaysFactory();
        using var factory = original.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IAttendanceEndpoint, ScopedDependencyEndpoint>()));
        var error = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        Assert.Contains(nameof(ICurrentUser), error.ToString());
        Assert.Contains("singleton", error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ScopedDependencyEndpoint : IAttendanceEndpoint
    {
        public ScopedDependencyEndpoint(ICurrentUser currentUser) { }
        public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
    }
}
