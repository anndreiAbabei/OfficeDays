using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Extensions;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Features.Users.DeleteRequirement.Contracts;

namespace OfficeDays.Features.Users.DeleteRequirement;

public sealed class DeleteRequirementHandler : IRequestHandler<DeleteRequirementRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DeleteRequirementHandler> _logger;
    
    public DeleteRequirementHandler(AppDbContext db, 
                                    ICurrentUser currentUser, 
                                    ILogger<DeleteRequirementHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(DeleteRequirementRequest request, CancellationToken cancellationToken)
    {
        var row = await _db.UserAttendanceRequirements.SingleOrDefaultAsync(x => x.Id == request.Id &&
                                                                                   x.UserId == _currentUser.Id, 
                                                                              cancellationToken);
        
        if (row is null)
        {
            _logger.LogRequirementNotFound(request.Id, _currentUser.Id);
            
            return Results.NotFound();
        }
        
        if (row.StartingFrom == UserAttendanceRequirement.BaselineDate)
        {
            _logger.LogBaselineChangeRejected(request.Id, _currentUser.Id);
            
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["id"] = ["The initial requirement cannot be deleted. You can edit its percentage."]
            });
        }
        
        _db.UserAttendanceRequirements.Remove(row);
        
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
        
        return Results.NoContent();
    }
}
