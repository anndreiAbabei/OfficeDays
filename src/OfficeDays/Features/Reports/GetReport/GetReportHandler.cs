using System.Data;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Reports.GetReport.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Reports.GetReport;

public sealed class GetReportHandler : IRequestHandler<GetReportRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;
    private readonly IReportCalculator _calculator;
    private readonly IHttpContextAccessor _context;
    private readonly ILogger<GetReportHandler> _logger;

    public GetReportHandler(AppDbContext db, 
                            ICurrentUser currentUser, 
                            TimeProvider clock,
                            IReportCalculator calculator, 
                            IHttpContextAccessor context, 
                            ILogger<GetReportHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _calculator = calculator;
        _context = context;
        _logger = logger;
    }

    public async ValueTask<IResult> Handle(GetReportRequest request, CancellationToken cancellationToken)
    {
        if (_context.HttpContext is { } context)
            context.Response.Headers.CacheControl = "no-store";

        // All inputs come from one database snapshot, even if another request edits attendance or requirements.
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        
        var user = await _db.Users
                            .AsNoTracking()
                            .SingleOrDefaultAsync(x => x.Id == _currentUser.Id, cancellationToken);
        
        if (user is null)
        {
            _logger.LogUserNotFound(_currentUser.Id);
            
            return Results.NotFound();
        }

        var generatedAt = _clock.GetUtcNow();
        var localNow = TimeZoneInfo.ConvertTime(generatedAt, TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId));
        var year = request.Year ?? localNow.Year;
        var startMonth = request.StartMonth ?? 1;
        var endMonth = request.EndMonth ?? 12;
        var start = new DateOnly(year, startMonth, 1);
        var end = new DateOnly(year, endMonth, DateTime.DaysInMonth(year, endMonth));
        var requirements = await _db.UserRequiredOfficePercentages
                                    .AsNoTracking()
                                    .Where(x => x.UserId == user.Id && x.StartingFrom <= end)
                                    .ToListAsync(cancellationToken);
        
        var holidays = await _db.BankHolidays
                                .Where(x => x.HolidayJurisdictionId == user.HolidayJurisdictionId && x.Date >= start && x.Date <= end)
                                .Select(x => x.Date)
                                .ToListAsync(cancellationToken);
        
        var vacations = await _db.Vacations
                                 .Where(x => x.UserId == user.Id && x.From <= end && x.To >= start)
                                 .Select(x => new { x.From, x.To })
                                 .ToListAsync(cancellationToken);
        var attendance = await _db.Attendances
                                  .Where(x => x.UserId == user.Id && x.Date >= start && x.Date <= end)
                                  .Select(x => x.Date)
                                  .ToListAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var report = _calculator.Calculate(year, startMonth, endMonth, user, generatedAt, requirements, holidays, 
                                           vacations.Select(x => (x.From, x.To)), attendance);
        
        _logger.LogReportGenerated(user.Id, year, startMonth, endMonth, report.Months.Count);
        
        return Results.Ok(report.ToViewModel());
    }
}
