using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;

namespace OfficeDays.Services;

public interface IOfficeRequirementReader
{
    Task<UserAttendanceRequirement> CurrentRequirement(User user, CancellationToken cancellationToken);
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
    
    public Task<UserAttendanceRequirement> CurrentRequirement(User user, CancellationToken cancellationToken)
    {
        var today = _dates.Today(user);
        return _db.UserAttendanceRequirements.AsNoTracking()
                  .Where(x => x.UserId == user.Id && x.StartingFrom <= today)
                  .OrderByDescending(x => x.StartingFrom)
                  .FirstAsync(cancellationToken);
    }

    public Task<int> Current(User user, CancellationToken cancellationToken) => ForDate(user.Id, _dates.Today(user), cancellationToken);

    public Task<int> ForDate(Guid userId, DateOnly date, CancellationToken cancellationToken)
    {
        return _db.UserAttendanceRequirements
                  .Where(x => x.UserId == userId && x.StartingFrom <= date)
                  .OrderByDescending(x => x.StartingFrom)
                  .Select(x => x.RequirementMode == AttendanceRequirementMode.DaysPerWeek ? x.RequirementValue * 20 : x.RequirementValue)
                  .FirstAsync(cancellationToken);
    }
}
