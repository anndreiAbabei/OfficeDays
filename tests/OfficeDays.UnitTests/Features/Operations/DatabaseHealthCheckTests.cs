using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OfficeDays.Data;
using OfficeDays.Features.Operations;

namespace OfficeDays.UnitTests.Features.Operations;

public sealed class DatabaseHealthCheckTests
{
    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Safe")]
    public async Task Should_ReturnHealthy_WhenDatabaseQuerySucceeds()
    {
        // arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();
        var sut = new DatabaseHealthCheck(provider.GetRequiredService<IServiceScopeFactory>());
        await using (var scope = provider.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
        }

        // act
        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        // assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Null(result.Exception);
    }

    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Safe")]
    public async Task Should_PropagateDatabaseFailure()
    {
        // arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();
        var sut = new DatabaseHealthCheck(provider.GetRequiredService<IServiceScopeFactory>());

        // act
        var result = await Assert.ThrowsAsync<SqliteException>(() => sut.CheckHealthAsync(new HealthCheckContext()));

        // assert
        Assert.Equal(1, result.SqliteErrorCode);
    }

    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Safe")]
    public async Task Should_HonorCancellation()
    {
        // arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();
        var sut = new DatabaseHealthCheck(provider.GetRequiredService<IServiceScopeFactory>());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // act
        var result = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sut.CheckHealthAsync(new HealthCheckContext(), cancellation.Token));

        // assert
        Assert.Equal(cancellation.Token, result.CancellationToken);
    }
}
