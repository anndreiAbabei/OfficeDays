using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Extensions;
using OfficeDays.Features.Planning.PlanDate.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Planning.PlanDate;

public sealed class PlanDateHandler : IRequestHandler<PlanDateRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;
    private readonly IPlanningCalendarReader _calendar;
    private readonly ILogger<PlanDateHandler> _logger;
    
    public PlanDateHandler(AppDbContext db, 
                           ICurrentUser currentUser, 
                           TimeProvider clock,
                           IPlanningCalendarReader calendar, 
                           ILogger<PlanDateHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _calendar = calendar;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(PlanDateRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync([_currentUser.Id], cancellationToken);
        
        if (user is null) 
        {
            _logger.LogUserNotFound(_currentUser.Id);
            return Results.NotFound();
        }
        
        var day = (await _calendar.Read(user, new CalculationPeriod(request.Date, request.Date, ""), cancellationToken)).Single();
        if (!day.IsEligible)
        {
            _logger.LogDateIneligible(user.Id, request.Date);
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["Date"] = ["Only eligible working days can be planned."] });
        }
        
        if (day.IsPlanned) 
            return Results.NoContent();
        
        var row = new OfficePlan
        {
            Id = Guid.NewGuid(), 
            UserId = user.Id,
            Date = request.Date, 
            CreatedAt = _clock.GetUtcNow()
        };
        await _db.OfficePlans.AddAsync(row, cancellationToken);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) 
            when (exception.IsUniqueViolation())
        {
            _db.Entry(row).State = EntityState.Detached;
            _logger.LogResolvedConcurrentUpdate(exception, user.Id, request.Date);
        }
        
        _logger.LogPlanChanged(user.Id, request.Date);
        
        return Results.NoContent();
    }
}
