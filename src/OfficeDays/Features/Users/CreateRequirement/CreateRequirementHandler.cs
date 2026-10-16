using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Extensions;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Features.Users.CreateRequirement.Contracts;

namespace OfficeDays.Features.Users.CreateRequirement;

public sealed class CreateRequirementHandler : IRequestHandler<CreateRequirementRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;
    private readonly ILogger<CreateRequirementHandler> _logger;
    
    public CreateRequirementHandler(AppDbContext db, 
                                    ICurrentUser currentUser, 
                                    TimeProvider clock, 
                                    ILogger<CreateRequirementHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(CreateRequirementRequest request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(x => x.Id == _currentUser.Id, cancellationToken);
        
        if (!userExists)
        {
            _logger.LogUserNotFound(_currentUser.Id);
            
            return Results.NotFound();
        }
        
        var row = new UserRequiredOfficePercentage
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.Id, 
            StartingFrom = request.Body.StartingFrom,
            RequiredOfficePercentage = request.Body.RequiredOfficePercentage, 
            CreatedAt = _clock.GetUtcNow()
        };
        
        await _db.UserRequiredOfficePercentages.AddAsync(row, cancellationToken);
        
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) 
            when (exception.IsUniqueViolation())
        {
            _logger.LogDuplicateRequirement(exception, _currentUser.Id, row.StartingFrom);
            
            return Results.Problem(statusCode: StatusCodes.Status409Conflict,
                                   title: "A requirement already exists for this month.", 
                                   detail: "Edit the existing requirement or choose another month.");
        }

        var result = row.ToViewModel();
        
        return Results.Created($"/api/users/me/requirements/{row.Id}", result);
    }
}
