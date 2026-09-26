using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Attendance.RemoveAttendance.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Attendance.RemoveAttendance;

public sealed class RemoveAttendanceHandler : IRequestHandler<RemoveAttendanceRequest>
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<RemoveAttendanceHandler> _logger;

    private static readonly Func<AppDbContext, Guid, DateOnly, CancellationToken, Task<Domain.Attendance?>> GetAttendanceQuery = 
        EF.CompileAsyncQuery((AppDbContext ctx, Guid userId, DateOnly date, CancellationToken ct) => ctx.Attendances.SingleOrDefault(x => x.UserId == userId && x.Date == date));

    public RemoveAttendanceHandler(AppDbContext dbContext, 
                                   IHttpContextAccessor contextAccessor,
                                   ILogger<RemoveAttendanceHandler> logger)
    {
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(RemoveAttendanceRequest request, CancellationToken cancellationToken)
    {
        var userId = _contextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;
        var attendanceDate = request.Date;
        var row = await GetAttendanceQuery(_dbContext, userId, attendanceDate, cancellationToken);
        
        if (row is null) 
            return Results.NotFound();
        
        _dbContext.Attendances.Remove(row);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogRemoveOfficeAttendance(userId, attendanceDate);
        
        return Results.NoContent();
    }
}
