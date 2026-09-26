using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Extensions;
using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

public sealed class CreateJurisdictionHandler : IRequestHandler<CreateJurisdictionRequest>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CreateJurisdictionHandler> _logger;
    
    public CreateJurisdictionHandler(AppDbContext dbContext,
                                     ILogger<CreateJurisdictionHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(CreateJurisdictionRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;
        HolidayJurisdictionCodes.TryNormalize(request.Code, out var code);
        
        if (await _dbContext.HolidayJurisdictions.AnyAsync(x => x.Code == code, cancellationToken))
        {
            _logger.LogDuplicateJurisdiction(code);
            
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict, 
                Title = "Holiday jurisdiction already exists"
            };
            return Results.Conflict(problem);
        }

        var jurisdiction = new HolidayJurisdiction
        {
            Code = code, 
            Name = request.Name.Trim()
        };
        await _dbContext.HolidayJurisdictions.AddAsync(jurisdiction, cancellationToken);
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.IsUniqueViolation())
        {
            _logger.LogDuplicateJurisdiction(code);
            
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict, 
                Title = "Holiday jurisdiction already exists"
            };
            return Results.Conflict(problem);
        }

        _logger.LogJurisdictionCreated(jurisdiction.Code);
        
        return Results.Created($"/api/holiday-jurisdictions/{jurisdiction.Code}", jurisdiction.ToViewModel());
    }
}
