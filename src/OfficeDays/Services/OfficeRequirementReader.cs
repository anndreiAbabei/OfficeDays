using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;

namespace OfficeDays.Services;

public interface IOfficeRequirementReader
{
    Task<int> Current(User user, CancellationToken cancellationToken);
    Task<int> ForDate(Guid userId, DateOnly date, CancellationToken cancellationToken);
}

public sealed class OfficeRequirementReader : IOfficeRequirementReader
{
    private readonly AppDbContext _db;
    private readonly IUserDateService _dates;
    
    public OfficeRequirementReader(AppDbContext db, IUserDateService dates)
    {
        _db = db;
        _dates = dates;
    }
    
    public Task<int> Current(User user, CancellationToken cancellationToken) => ForDate(user.Id, _dates.Today(user), cancellationToken);

    public Task<int> ForDate(Guid userId, DateOnly date, CancellationToken cancellationToken)
    {
        return _db.UserRequiredOfficePercentages
                  .Where(x => x.UserId == userId && x.StartingFrom <= date)
                  .OrderByDescending(x => x.StartingFrom)
                  .Select(x => x.RequiredOfficePercentage)
                  .FirstAsync(cancellationToken);
    }
}
