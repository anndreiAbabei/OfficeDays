using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Vacations.UpdateVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.UpdateVacation;

public sealed class UpdateVacationHandler : IRequestHandler<UpdateVacationRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UpdateVacationHandler> _logger;

    public UpdateVacationHandler(ICurrentUser currentUser,
                                 AppDbContext dbContext,
                                 ILogger<UpdateVacationHandler> logger)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async ValueTask<IResult> Handle(UpdateVacationRequest input, CancellationToken cancellationToken)
    {
        var vacation = await _dbContext.Vacations.SingleOrDefaultAsync(
            x => x.Id == input.Id && x.UserId == _currentUser.Id, cancellationToken);

        if (vacation is null)
        {
            _logger.LogVacationNotFound(input.Id, _currentUser.Id);
            return Results.NotFound();
        }

        vacation.From = input.Body.From;
        vacation.To = input.Body.To;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogVacationUpdated(vacation.UserId, vacation.Id, vacation.From, vacation.To);

        return Results.NoContent();
    }
}
