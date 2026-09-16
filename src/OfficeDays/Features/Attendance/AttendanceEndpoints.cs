using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Common;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Attendance;

public static class AttendanceEndpoints
{
    private const string LogCategory = "OfficeDays.Features.Attendance";

    public static void Map(RouteGroupBuilder api)
    {
        var attendance = api.MapGroup("/attendance").RequireAuthorization();
        attendance.MapPost("/", RecordToday).AddEndpointFilter<CookieAntiforgeryFilter>();
        attendance.MapPut("/{date}", RecordDate).AddEndpointFilter<CookieAntiforgeryFilter>();
        attendance.MapGet("/", GetAttendance);
        attendance.MapDelete("/{date}", RemoveAttendance).AddEndpointFilter<CookieAntiforgeryFilter>();
    }

    private static async Task<IResult> RecordToday(RecordAttendanceRequest? request,
                                                   ClaimsPrincipal principal,
                                                   AppDbContext db,
                                                   UserDateService dates,
                                                   TimeProvider clock,
                                                   ILoggerFactory loggerFactory,
                                                   CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([principal.GetUserId()], cancellationToken);
        
        return user is not null
            ? await Add(user.Id, dates.Today(user), request?.IsManual ?? false, db, clock,
                loggerFactory.CreateLogger(LogCategory), cancellationToken)
            : Results.NotFound();
    }

    private static async Task<IResult> RecordDate(string date,
                                                  ClaimsPrincipal principal,
                                                  AppDbContext db,
                                                  UserDateService dates,
                                                  TimeProvider clock,
                                                  ILoggerFactory loggerFactory,
                                                  CancellationToken cancellationToken)
    {
        if (!AttendanceValidation.TryParseDate(date, out var attendanceDate))
            return ApiResults.Validation("Date must use the yyyy-MM-dd format.", "date");
        
        var user = await db.Users.FindAsync([principal.GetUserId()], cancellationToken);
        
        if (user is null) 
            return Results.NotFound();
        
        if (attendanceDate > dates.Today(user))
            return ApiResults.Validation("Future attendance cannot be recorded.", "date");
        
        return await Add(user.Id, attendanceDate, true, db, clock,
            loggerFactory.CreateLogger(LogCategory), cancellationToken);
    }

    private static async Task<IResult> GetAttendance(int? year,
                                                     int? month,
                                                     ClaimsPrincipal principal,
                                                     AppDbContext db,
                                                     CancellationToken cancellationToken)
    {
        var query = db.Attendances.AsNoTracking()
                      .Where(x => x.UserId == principal.GetUserId());
        
        if (year.HasValue || month.HasValue)
        {
            if (!ApiResults.TryMonth(year, month, out var period, out var error)) 
                return error;
            
            query = query.Where(x => x.Date >= period.Start && x.Date <= period.End);
        }

        var rows = await query.OrderByDescending(x => x.Date).ToListAsync(cancellationToken);
        
        return Results.Ok(rows.Select(x => x.ToViewModel()));
    }

    private static async Task<IResult> RemoveAttendance(string date,
                                                        ClaimsPrincipal principal,
                                                        AppDbContext db,
                                                        ILoggerFactory loggerFactory,
                                                        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        
        if (!AttendanceValidation.TryParseDate(date, out var attendanceDate))
            return ApiResults.Validation("Date must use the yyyy-MM-dd format.", "date");
        
        var userId = principal.GetUserId();
        var row = await db.Attendances.SingleOrDefaultAsync(
            x => x.UserId == userId && x.Date == attendanceDate, cancellationToken);
        
        if (row is null) 
            return Results.NotFound();
        
        db.Attendances.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("User {UserId} removed office attendance for {AttendanceDate}", userId, attendanceDate);
        
        return Results.NoContent();
    }

    private static async Task<IResult> Add(Guid userId,
                                           DateOnly date,
                                           bool isManual,
                                           AppDbContext db,
                                           TimeProvider clock,
                                           ILogger logger,
                                           CancellationToken cancellationToken)
    {
        var existing = await db.Attendances.SingleOrDefaultAsync(
            x => x.UserId == userId && x.Date == date, cancellationToken);

        if (existing is not null)
        {
            logger.LogDebug("Ignored duplicate office attendance for user {UserId} on {AttendanceDate}", userId, date);
            return Results.Ok(existing.ToViewModel());
        }

        var row = new Domain.Attendance
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = date,
            IsManual = isManual,
            CreatedAt = clock.GetUtcNow()
        };
        db.Attendances.Add(row);

        try
        {
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Recorded office attendance for user {UserId} on {AttendanceDate}", userId, date);
            return Results.Created($"/api/attendance/{date:yyyy-MM-dd}", row.ToViewModel());
        }
        catch (DbUpdateException exception)
            when (ApiResults.IsUniqueViolation(exception))
        {
            db.Entry(row).State = EntityState.Detached;
            existing = await db.Attendances.SingleAsync(
                x => x.UserId == userId && x.Date == date, cancellationToken);

            logger.LogDebug("Resolved concurrent duplicate attendance for user {UserId} on {AttendanceDate}", userId, date);
            
            return Results.Ok(existing.ToViewModel());
        }
    }
}
