using OfficeDays.Data;
using OfficeDays.Features.Planning.GetPlanning.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Planning.GetPlanning;

public sealed class GetPlanningHandler : IRequestHandler<GetPlanningRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IUserDateService _dates;
    private readonly IPlanningCalendarReader _calendar;
    private readonly ILogger<GetPlanningHandler> _logger;
    
    public GetPlanningHandler(AppDbContext db, 
                              ICurrentUser currentUser,
                              IUserDateService dates,
                              IPlanningCalendarReader calendar,
                              ILogger<GetPlanningHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _dates = dates;
        _calendar = calendar;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(GetPlanningRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync([_currentUser.Id], cancellationToken);
        
        if (user is null)
        {
            _logger.LogUserNotFound(_currentUser.Id);
            return Results.NotFound();
        }
        
        var today = _dates.Today(user);
        var days = await _calendar.Read(user, CalculationPeriod.ForMonth(request.Year, request.Month), cancellationToken);
        var response = new GetPlanningResponse(today, PlanningCalculator.UpcomingCount(days, today), days);
        
        return Results.Ok(response);
    }
}
