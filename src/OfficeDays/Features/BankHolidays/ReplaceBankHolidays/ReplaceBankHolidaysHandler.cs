using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

public sealed class ReplaceBankHolidaysHandler : IRequestHandler<ReplaceBankHolidaysRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ReplaceBankHolidaysHandler> _logger;
    
    public ReplaceBankHolidaysHandler(ICurrentUser currentUser,
                                      AppDbContext dbContext,
                                      ILogger<ReplaceBankHolidaysHandler> logger)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(ReplaceBankHolidaysRequest input, CancellationToken cancellationToken)
    {
        var countryCode = input.CountryCode;
        var year = input.Year;
        var request = input.Body;
        HolidayJurisdictionCodes.TryNormalize(countryCode, out var normalizedCode);

        var jurisdiction = await _dbContext.HolidayJurisdictions.SingleOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);
        
        if (jurisdiction is null)
        {
            _logger.LogJurisdictionNotFound(normalizedCode);
            return Results.NotFound();
        }

        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);
        
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.BankHolidays
                 .Where(x => x.HolidayJurisdictionId == jurisdiction.Id &&
                             x.Date >= start && x.Date <= end)
                 .ExecuteDeleteAsync(cancellationToken);
        
        var rows = request.Select(x => x.ToEntity(jurisdiction)).ToList();
        _dbContext.BankHolidays.AddRange(rows);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        
        _logger.LogHolidaysReplaced(_currentUser.Id, jurisdiction.Code, year, rows.Count);
        
        return Results.Ok(rows.OrderBy(x => x.Date).ToViewModel());
    }
}
