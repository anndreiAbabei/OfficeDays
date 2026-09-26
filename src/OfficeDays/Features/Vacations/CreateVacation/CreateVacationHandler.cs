using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Vacations.CreateVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.CreateVacation;

public sealed class CreateVacationHandler(ICurrentUser currentUser,
    AppDbContext db,
    TimeProvider clock,
    ILogger<CreateVacationHandler> logger) : IRequestHandler<CreateVacationRequest>
{
    public async ValueTask<IResult> Handle(CreateVacationRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;

        var row = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            From = request.From,
            To = request.To,
            CreatedAt = clock.GetUtcNow()
        };
        db.Vacations.Add(row);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogVacationCreated(row.UserId, row.Id, row.From, row.To);
        return Results.Created($"/api/vacations/{row.Id}", row.ToViewModel());
    }
}
