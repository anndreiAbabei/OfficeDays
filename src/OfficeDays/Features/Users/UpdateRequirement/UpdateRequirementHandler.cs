using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Extensions;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementHandler(AppDbContext db, ICurrentUser currentUser, ILogger<UpdateRequirementHandler> logger) : IRequestHandler<UpdateRequirementRequest>
{
    public async ValueTask<IResult> Handle(UpdateRequirementRequest request, CancellationToken cancellationToken)
    {
        var row = await db.UserRequiredOfficePercentages.SingleOrDefaultAsync(x => x.Id == request.Id && 
                                                                                   x.UserId == currentUser.Id, 
                                                                              cancellationToken);
        
        if (row is null)
        {
            logger.LogRequirementNotFound(request.Id, currentUser.Id);
            return Results.NotFound();
        }
        
        if (row.StartingFrom == UserRequiredOfficePercentage.BaselineDate != (request.Body.StartingFrom == UserRequiredOfficePercentage.BaselineDate))
        {
            logger.LogBaselineChangeRejected(request.Id, currentUser.Id);
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["startingFrom"] = ["The initial requirement's starting month cannot be changed or assigned to another entry."]
            });
        }
        
        row.StartingFrom = request.Body.StartingFrom;
        row.RequiredOfficePercentage = request.Body.RequiredOfficePercentage;
        
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) 
            when (exception.IsUniqueViolation())
        {
            logger.LogDuplicateRequirement(exception, currentUser.Id, row.StartingFrom);
            return Results.Problem(statusCode: StatusCodes.Status409Conflict,
                                   title: "A requirement already exists for this month.", 
                                   detail: "Edit the existing requirement or choose another month.");
        }
        var result = row.ToViewModel();
        
        return Results.Ok(result);
    }
}
