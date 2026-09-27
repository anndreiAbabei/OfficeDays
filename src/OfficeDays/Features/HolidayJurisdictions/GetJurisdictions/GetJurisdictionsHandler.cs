using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions.GetJurisdictions;

public sealed class GetJurisdictionsHandler : IRequestHandler<GetJurisdictionsRequest>
{
    private readonly AppDbContext _dbContext;
    
    public GetJurisdictionsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async ValueTask<IResult> Handle(GetJurisdictionsRequest input, CancellationToken cancellationToken)
    {
        var rows = await _dbContext.HolidayJurisdictions
                                   .AsNoTracking()
                                   .OrderBy(x => x.Name)
                                   .ToListAsync(cancellationToken);

        var result = rows.ToViewModel();

        return Results.Ok(result);
    }
}
