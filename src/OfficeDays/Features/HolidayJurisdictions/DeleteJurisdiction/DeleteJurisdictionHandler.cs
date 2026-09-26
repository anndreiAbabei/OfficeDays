using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;

public sealed class DeleteJurisdictionHandler : IRequestHandler<DeleteJurisdictionRequest>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DeleteJurisdictionHandler> _logger;
    
    public DeleteJurisdictionHandler(AppDbContext dbContext, ILogger<DeleteJurisdictionHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(DeleteJurisdictionRequest input, CancellationToken cancellationToken)
    {
        var code = input.Code;
        HolidayJurisdictionCodes.TryNormalize(code, out var normalizedCode);

        var jurisdiction = await _dbContext.HolidayJurisdictions.SingleOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);
        
        if (jurisdiction is null) 
            return Results.NotFound();

        var inUse = await _dbContext.Users.AnyAsync(x => x.HolidayJurisdictionId == jurisdiction.Id, cancellationToken) ||
                    await _dbContext.BankHolidays.AnyAsync(x => x.HolidayJurisdictionId == jurisdiction.Id, cancellationToken);
        if (inUse)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Holiday jurisdiction is in use",
                Detail = "Remove its bank holidays and user references before deleting it."
            };
            
            return Results.Conflict(problem);
        }

        _dbContext.HolidayJurisdictions.Remove(jurisdiction);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogJurisdictionDeleted(jurisdiction.Code);
        
        return Results.NoContent();
    }
}
