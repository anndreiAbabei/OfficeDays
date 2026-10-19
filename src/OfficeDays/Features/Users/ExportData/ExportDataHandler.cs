using System.Data;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Users.ExportData.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Users.ExportData;

public sealed class ExportDataHandler : IRequestHandler<ExportDataRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IAppVersionService _appVersion;
    private readonly ILogger<ExportDataHandler> _logger;
    
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
    
    public ExportDataHandler(AppDbContext db, 
                             ICurrentUser currentUser, 
                             TimeProvider clock,
                             IHttpContextAccessor contextAccessor,
                             IAppVersionService appVersion,
                             ILogger<ExportDataHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _contextAccessor = contextAccessor;
        _appVersion = appVersion;
        _logger = logger;
    }

    public async ValueTask<IResult> Handle(ExportDataRequest request, CancellationToken cancellationToken)
    {
        if (_contextAccessor.HttpContext is { } context)
            context.Response.Headers.CacheControl = "no-store";

        // A single consistent snapshot includes all years, even past plans and future requirements.
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var profile = await _db.Users
                               .AsNoTracking()
                               .Where(x => x.Id == _currentUser.Id)
                               .Select(x => new ExportProfile(x.Id,
                                                              x.Username, 
                                                              x.NormalizedUsername, 
                                                              x.Email, 
                                                              x.TimeZoneId,
                                                              x.HolidayJurisdictionId, 
                                                              x.IsAdmin, 
                                                              x.CreatedAt))
                               .SingleOrDefaultAsync(cancellationToken);
        
        if (profile is null)
        {
            _logger.LogUserNotFound(_currentUser.Id);

            return Results.Problem(statusCode: 404, detail: "Your account could not be found. Please sign in again.");
        }

        var teamDay = await _db.Users
                               .Where(x => x.Id == _currentUser.Id)
                               .Select(x => x.Preferences.TeamDay)
                               .SingleAsync(cancellationToken);
        
        var requirements = await _db.UserRequiredOfficePercentages
                                    .Where(x => x.UserId == _currentUser.Id)
                                    .OrderBy(x => x.StartingFrom)
                                    .Select(x => new ExportRequirement(x.Id, 
                                                                       x.UserId, 
                                                                       x.StartingFrom,
                                                                       x.RequiredOfficePercentage, 
                                                                       x.CreatedAt))
                                    .ToArrayAsync(cancellationToken);
        
        var attendance = await _db.Attendances
                                  .Where(x => x.UserId == _currentUser.Id)
                                  .OrderBy(x => x.Date)
                                  .Select(x => new ExportAttendance(x.Id, 
                                                                    x.UserId, 
                                                                    x.Date, 
                                                                    x.IsManual, 
                                                                    x.CreatedAt))
                                  .ToArrayAsync(cancellationToken);
        
        var plans = await _db.OfficePlans
                             .Where(x => x.UserId == _currentUser.Id)
                             .OrderBy(x => x.Date)
                             .Select(x => new ExportPlan(x.Id, x.UserId, x.Date, x.CreatedAt))
                             .ToArrayAsync(cancellationToken);
        
        var vacations = await _db.Vacations
                                 .Where(x => x.UserId == _currentUser.Id)
                                 .OrderBy(x => x.From)
                                 .ThenBy(x => x.To)
                                 .ThenBy(x => x.Id)
                                 .Select(x => new ExportVacation(x.Id, x.UserId, x.From, x.To, x.CreatedAt))
                                 .ToArrayAsync(cancellationToken);
        
        var calendar = await _db.HolidayJurisdictions
                                .Where(x => x.Id == profile.HolidayJurisdictionId)
                                .Select(x => new { x.Id, x.Code, x.Name })
                                .SingleAsync(cancellationToken);
        
        var holidays = await _db.BankHolidays.Where(x => x.HolidayJurisdictionId == calendar.Id)
                                .OrderBy(x => x.Date)
                                .Select(x => new ExportHoliday(x.Id, x.HolidayJurisdictionId, x.Date, x.Name))
                                .ToArrayAsync(cancellationToken);
        
        var exportedAt = _clock.GetUtcNow();
        await transaction.CommitAsync(cancellationToken);

        var zone = TimeZoneInfo.FindSystemTimeZoneById(profile.TimeZoneId);
        var export = new PersonalDataExport("officedays-personal-data", 1, exportedAt, 
                                            new ExportApplication("OfficeDays", _appVersion.Version),
                                            new ExportReportContext(1, DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(exportedAt, zone).DateTime), 
                                                                       DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(profile.CreatedAt, zone).DateTime)), 
                                            profile, 
                                            new ExportPreferences(teamDay?.ToString().ToLowerInvariant()), 
                                            requirements, 
                                            attendance, 
                                            plans, 
                                            vacations, 
                                            new ExportHolidayCalendar(calendar.Id, calendar.Code, calendar.Name, holidays));
        
        return Results.File(JsonSerializer.SerializeToUtf8Bytes(export, _jsonOptions), 
                            MediaTypeNames.Application.Json, 
                            $"officedays-data-{exportedAt:yyyy-MM-dd-HHmmss}.json");
    }
}
