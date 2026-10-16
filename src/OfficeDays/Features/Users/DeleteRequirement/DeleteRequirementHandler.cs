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
    
    public DeleteRequirementHandler(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }
    
    public async ValueTask<IResult> Handle(DeleteRequirementRequest request, CancellationToken cancellationToken)
    {
        var row = await _db.UserRequiredOfficePercentages.SingleOrDefaultAsync(x => x.Id == request.Id && 
                                                                                   x.UserId == _currentUser.Id, 
                                                                              cancellationToken);
        
        if (row is null) 
            return Results.NotFound();
        
        if (row.StartingFrom == UserRequiredOfficePercentage.BaselineDate)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["id"] = ["The initial requirement cannot be deleted. You can edit its percentage."]
            });
        
        _db.UserRequiredOfficePercentages.Remove(row);
        
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) 
            when (exception.IsUniqueViolation())
        {
            return Results.Problem(statusCode: StatusCodes.Status409Conflict,
                                   title: "A requirement already exists for this month.", 
                                   detail: "Edit the existing requirement or choose another month.");
        }
        
        return Results.NoContent();
    }
}
