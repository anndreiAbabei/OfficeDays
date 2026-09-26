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
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<GetAttendanceHandler> _logger;

    private static readonly Func<AppDbContext, Guid, IAsyncEnumerable<Domain.Attendance>> GetAttendanceQuery = 
        EF.CompileAsyncQuery((AppDbContext ctx, Guid userId) => ctx.Attendances
                                                                   .AsNoTracking()
                                                                   .Where(x => x.UserId == userId));

    public GetAttendanceHandler(AppDbContext dbContext, 
                                IHttpContextAccessor contextAccessor,
                                ILogger<GetAttendanceHandler> logger)
    {
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(GetAttendanceRequest request, CancellationToken cancellationToken)
    {
        var userId = _contextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;
        var query = GetAttendanceQuery(_dbContext, userId);
        
        var year = request.Year;
        var month = request.Month;
        
        CalculationPeriod? period = null;
        if (year.HasValue && month.HasValue)
        {
            period = CalculationPeriod.ForMonth(year.Value, month.Value);

            query = query.Where(x => x.Date >= period.Value.Start && x.Date <= period.Value.End);
        }
        
        var rows = await query.OrderByDescending(x => x.Date).ToListAsync(cancellationToken);
        
        _logger.LogGetEntries(userId, period);
        
        return Results.Ok(rows.ToViewModel());
    }
}
