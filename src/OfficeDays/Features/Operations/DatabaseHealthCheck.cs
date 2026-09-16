using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OfficeDays.Data;

namespace OfficeDays.Features.Operations;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    public DatabaseHealthCheck(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Execute a real query against the application schema, including on an empty table.
        await db.Users.AsNoTracking().Select(user => user.Id).Take(1).ToListAsync(cancellationToken);
        
        return HealthCheckResult.Healthy();
    }
}
