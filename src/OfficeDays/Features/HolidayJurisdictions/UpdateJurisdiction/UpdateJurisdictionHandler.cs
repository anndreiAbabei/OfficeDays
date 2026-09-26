using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

public sealed class UpdateJurisdictionHandler : IRequestHandler<UpdateJurisdictionRequest>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UpdateJurisdictionHandler> _logger;
    
    public UpdateJurisdictionHandler(AppDbContext dbContext, ILogger<UpdateJurisdictionHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(UpdateJurisdictionRequest input, CancellationToken cancellationToken)
    {
        var code = input.Code;
        var request = input.Body;
        HolidayJurisdictionCodes.TryNormalize(code, out var normalizedCode);

        var jurisdiction = await _dbContext.HolidayJurisdictions.SingleOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);
        
        if (jurisdiction is null)
            return Results.NotFound();

        jurisdiction.Name = request.Name!.Trim();
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogJurisdictionUpdated(jurisdiction.Code);
        
        return Results.Ok(jurisdiction.ToViewModel());
    }
}
