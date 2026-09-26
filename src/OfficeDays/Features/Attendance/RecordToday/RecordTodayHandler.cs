using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Attendance.RecordToday.Contracts;
using OfficeDays.Features.Common;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Attendance.RecordToday;

public sealed class RecordTodayHandler : IRequestHandler<RecordTodayRequest>
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserDateService _dates;
    private readonly TimeProvider _clock;
    private readonly ILogger<RecordTodayHandler> _logger;
    
    private static readonly Func<AppDbContext, Guid, DateOnly, CancellationToken, Task<Domain.Attendance?>> GetAttendanceQuery = 
        EF.CompileAsyncQuery((AppDbContext ctx, Guid userId, DateOnly date, CancellationToken ct) => ctx.Attendances.SingleOrDefault(x => x.UserId == userId && x.Date == date));


    public RecordTodayHandler(AppDbContext dbContext, 
                              IHttpContextAccessor contextAccessor,
                              IUserDateService dates,
                              TimeProvider clock,
                              ILogger<RecordTodayHandler> logger)
    {
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _dates = dates;
        _clock = clock;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(RecordTodayRequest request, CancellationToken cancellationToken)
    {
        var userId = _contextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;
        var user = await _dbContext.Users.FindAsync([userId], cancellationToken);

        if (user == null)
            return Results.NotFound();
        
        var date = _dates.Today(user);
        var existing = await GetAttendanceQuery(_dbContext, userId, date, cancellationToken);

        if (existing is not null)
        {
            _logger.LogDuplicatedEntryRequest(userId, date);
            return Results.Ok(existing.ToViewModel());
        }

        var row = new Domain.Attendance
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = date,
            IsManual = request.Body?.IsManual ?? false,
            CreatedAt = _clock.GetUtcNow()
        };
        await _dbContext.Attendances.AddAsync(row, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogEntryCreated(userId, date);
            return Results.Created($"/api/attendance/{date:yyyy-MM-dd}", row.ToViewModel());
        }
        catch (DbUpdateException exception)
            when (ApiResults.IsUniqueViolation(exception))
        {
            _dbContext.Entry(row).State = EntityState.Detached;
            existing = await GetAttendanceQuery(_dbContext, userId, date, cancellationToken);

            _logger.LogResolvedConcurrentUpdate(userId, date);
            
            return Results.Ok(existing!.ToViewModel());
        }
    }
}
