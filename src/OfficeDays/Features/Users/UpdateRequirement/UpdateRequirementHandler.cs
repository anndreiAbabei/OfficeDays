using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Extensions;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Features.Users.Requirements;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementHandler(AppDbContext db, ICurrentUser currentUser) : IRequestHandler<UpdateRequirementRequest>
{
    public async ValueTask<IResult> Handle(UpdateRequirementRequest request, CancellationToken cancellationToken)
    {
        var row = await db.UserRequiredOfficePercentages.SingleOrDefaultAsync(x => x.Id == request.Id && 
                                                                                   x.UserId == currentUser.Id, 
                                                                              cancellationToken);
        
        if (row is null) 
            return Results.NotFound();
        
        if (row.StartingFrom == UserRequiredOfficePercentage.BaselineDate != (request.Body.StartingFrom == UserRequiredOfficePercentage.BaselineDate))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["startingFrom"] = ["The initial requirement's starting month cannot be changed or assigned to another entry."]
            });
        
        row.StartingFrom = request.Body.StartingFrom;
        row.RequiredOfficePercentage = request.Body.RequiredOfficePercentage;
        
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) 
            when (exception.IsUniqueViolation())
        {
            return Results.Problem(statusCode: StatusCodes.Status409Conflict,
                                   title: "A requirement already exists for this month.", 
                                   detail: "Edit the existing requirement or choose another month.");
        }
        var result = RequirementResponse.From(row);
        
        return Results.Ok(result);
    }
}
