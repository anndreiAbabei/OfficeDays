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

    private static async Task<IResult> RecordToday(ClaimsPrincipal principal,
                                                   AppDbContext db,
                                                   UserDateService dates,
                                                   TimeProvider clock,
                                                   ILoggerFactory loggerFactory)
    {
        var user = await db.Users.FindAsync(principal.GetUserId());
        
        return user is null
            ? Results.NotFound()
            : await Add(user.Id, dates.Today(user), db, clock, loggerFactory.CreateLogger(LogCategory));
    }

    private static async Task<IResult> RecordDate(string date,
                                                  ClaimsPrincipal principal,
                                                  AppDbContext db,
                                                  UserDateService dates,
                                                  TimeProvider clock,
                                                  ILoggerFactory loggerFactory)
    {
        if (!AttendanceValidation.TryParseDate(date, out var attendanceDate))
            return ApiResults.Validation("Date must use the yyyy-MM-dd format.", "date");
        
        var user = await db.Users.FindAsync(principal.GetUserId());
        
        if (user is null) 
            return Results.NotFound();
        
        if (attendanceDate > dates.Today(user))
            return ApiResults.Validation("Future attendance cannot be recorded.", "date");
        
        return await Add(user.Id, attendanceDate, db, clock, loggerFactory.CreateLogger(LogCategory));
    }

    private static async Task<IResult> GetAttendance(int? year,
                                                     int? month,
                                                     ClaimsPrincipal principal,
                                                     AppDbContext db)
    {
        var query = db.Attendances.AsNoTracking()
                      .Where(x => x.UserId == principal.GetUserId());
        
        if (year.HasValue || month.HasValue)
        {
            if (!ApiResults.TryMonth(year, month, out var period, out var error)) 
                return error;
            
            query = query.Where(x => x.Date >= period.Start && x.Date <= period.End);
        }

        var rows = await query.OrderByDescending(x => x.Date).ToListAsync();
        
        return Results.Ok(rows.Select(x => x.ToViewModel()));
    }

    private static async Task<IResult> RemoveAttendance(string date,
                                                        ClaimsPrincipal principal,
                                                        AppDbContext db,
                                                        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        
        if (!AttendanceValidation.TryParseDate(date, out var attendanceDate))
            return ApiResults.Validation("Date must use the yyyy-MM-dd format.", "date");
        
        var userId = principal.GetUserId();
        var row = await db.Attendances.SingleOrDefaultAsync(x => x.UserId == userId && x.Date == attendanceDate);
        
        if (row is null) 
            return Results.NotFound();
        
        db.Attendances.Remove(row);
        await db.SaveChangesAsync();
        
        logger.LogInformation("User {UserId} removed office attendance for {AttendanceDate}", userId, attendanceDate);
        
        return Results.NoContent();
    }

    private static async Task<IResult> Add(Guid userId,
                                           DateOnly date,
                                           AppDbContext db,
                                           TimeProvider clock,
                                           ILogger logger)
    {
        var existing = await db.Attendances.SingleOrDefaultAsync(x => x.UserId == userId && x.Date == date);

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
            CreatedAt = clock.GetUtcNow()
        };
        db.Attendances.Add(row);

        try
        {
            await db.SaveChangesAsync();

            logger.LogInformation("Recorded office attendance for user {UserId} on {AttendanceDate}", userId, date);
            return Results.Created($"/api/attendance/{date:yyyy-MM-dd}", row.ToViewModel());
        }
        catch (DbUpdateException exception)
            when (ApiResults.IsUniqueViolation(exception))
        {
            db.Entry(row).State = EntityState.Detached;
            existing = await db.Attendances.SingleAsync(x => x.UserId == userId && x.Date == date);

            logger.LogDebug("Resolved concurrent duplicate attendance for user {UserId} on {AttendanceDate}", userId, date);
            return Results.Ok(existing.ToViewModel());
        }
    }
}
