using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Vacations.GetVacations.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Vacations.GetVacations;

public sealed class GetVacationsHandler : IRequestHandler<GetVacationsRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    
    public GetVacationsHandler(ICurrentUser currentUser,
                               AppDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }
    
    public async ValueTask<IResult> Handle(GetVacationsRequest input, CancellationToken cancellationToken)
    {
        var year = input.Year;
        var month = input.Month;
        var query = _dbContext.Vacations
                              .AsNoTracking()
                              .Where(x => x.UserId == _currentUser.Id);

        if (year.HasValue && month.HasValue)
        {
            var period = CalculationPeriod.ForMonth(year.Value, month.Value);
            
            query = query.Where(x => x.From <= period.End && x.To >= period.Start);
        }

        var rows = await query.OrderByDescending(x => x.From)
                              .ToListAsync(cancellationToken);

        return Results.Ok(rows.ToViewModel());
    }
}
