using System.Net;
using System.Text.Json;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

namespace OfficeDays.IntegrationTests;

public sealed class OperationsIntegrationTests
{
    [Fact]
    public async Task Version_is_public_matches_build_and_is_not_cached()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/version");
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion, json.RootElement.GetProperty("version").GetString());
        Assert.True(response.Headers.CacheControl!.NoStore);
        Assert.Contains("id=\"app-version\"", await client.GetStringAsync("/"));
    }

    [Fact]
    public async Task Readiness_checks_database_while_liveness_survives_database_failure()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var healthy = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, healthy.StatusCode);
        Assert.Contains("\"database\":\"Healthy\"", await healthy.Content.ReadAsStringAsync());

        // Break only this test's isolated database after successful application startup.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Users RENAME TO UnavailableUsers");
        var unhealthy = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, unhealthy.StatusCode);
        Assert.Contains("\"database\":\"Unhealthy\"", await unhealthy.Content.ReadAsStringAsync());
        Assert.DoesNotContain("SQLite", await unhealthy.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
    }
}
