using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Extensions;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementHandler : IRequestHandler<UpdateRequirementRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateRequirementHandler> _logger;

    public UpdateRequirementHandler(AppDbContext db, ICurrentUser currentUser, ILogger<UpdateRequirementHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async ValueTask<IResult> Handle(UpdateRequirementRequest request, CancellationToken cancellationToken)
    {
        var row = await _db.UserAttendanceRequirements.SingleOrDefaultAsync(x => x.Id == request.Id &&
                                                                                 x.UserId == _currentUser.Id,
                                                                              cancellationToken);
        
        if (row is null)
        {
            _logger.LogRequirementNotFound(request.Id, _currentUser.Id);
            return Results.NotFound();
        }
        
        if (row.StartingFrom == UserAttendanceRequirement.BaselineDate != (request.Body.StartingFrom == UserAttendanceRequirement.BaselineDate))
        {
            _logger.LogBaselineChangeRejected(request.Id, _currentUser.Id);
            
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["startingFrom"] = ["The initial requirement's starting month cannot be changed or assigned to another entry."]
            });
        }
        
        row.StartingFrom = request.Body.StartingFrom;
        row.Mode = request.Body.Mode ?? AttendanceRequirementMode.Percentage;
        row.Value = request.Body.Value ?? request.Body.RequiredOfficePercentage.GetValueOrDefault();
        
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
        
        return Results.Ok(result);
    }
}
