using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Vacations.CreateVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.CreateVacation;

public sealed class CreateVacationHandler : IRequestHandler<CreateVacationRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    private readonly TimeProvider _clock;
    private readonly ILogger<CreateVacationHandler> _logger;
    
    public CreateVacationHandler(ICurrentUser currentUser,
                                 AppDbContext dbContext,
                                 TimeProvider clock,
                                 ILogger<CreateVacationHandler> logger)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
        _clock = clock;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(CreateVacationRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;

        var row = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.Id,
            From = request.From,
            To = request.To,
            CreatedAt = _clock.GetUtcNow()
        };
        
        await _dbContext.Vacations.AddAsync(row, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogVacationCreated(row.UserId, row.Id, row.From, row.To);
        
        return Results.Created($"/api/vacations/{row.Id}", row.ToViewModel());
    }
}
