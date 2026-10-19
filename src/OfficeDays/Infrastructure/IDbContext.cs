namespace OfficeDays.Infrastructure;

public interface IDbContext
{
    ValueTask<TEntity?> FindAsync<TEntity>(object?[]? keyValues, CancellationToken cancellationToken) 
        where TEntity : class;
}
