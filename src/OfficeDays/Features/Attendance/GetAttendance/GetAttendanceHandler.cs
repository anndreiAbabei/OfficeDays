using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Attendance.GetAttendance.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Attendance.GetAttendance;

public sealed class GetAttendanceHandler : IRequestHandler<GetAttendanceRequest>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<GetAttendanceHandler> _logger;

    private static readonly Func<AppDbContext, Guid, DateOnly, DateOnly, IAsyncEnumerable<Domain.Attendance>> GetAttendanceQuery =
        EF.CompileAsyncQuery<AppDbContext, Guid, DateOnly, DateOnly, Domain.Attendance>(
            (ctx, userId, start, end) => ctx.Attendances
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Date >= start && x.Date <= end)
                .OrderByDescending(x => x.Date));

    public GetAttendanceHandler(AppDbContext dbContext,
                                ICurrentUser currentUser,
                                ILogger<GetAttendanceHandler> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async ValueTask<IResult> Handle(GetAttendanceRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id;

        var year = request.Year;
        var month = request.Month;

        CalculationPeriod? period = null;
        if (year.HasValue && month.HasValue)
        {
            period = CalculationPeriod.ForMonth(year.Value, month.Value);
        }

        var query = GetAttendanceQuery(_dbContext, userId,
            period?.Start ?? DateOnly.MinValue, period?.End ?? DateOnly.MaxValue);
        var rows = await query.ToListAsync(cancellationToken);

        _logger.LogGetEntries(userId, period);

        return Results.Ok(rows.ToViewModel());
    }
}
