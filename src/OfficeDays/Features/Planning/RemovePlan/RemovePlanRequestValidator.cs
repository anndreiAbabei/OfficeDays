using FluentValidation;
using OfficeDays.Data;
using OfficeDays.Features.Planning.RemovePlan.Contracts;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Planning.RemovePlan;

public sealed class RemovePlanRequestValidator : AbstractValidator<RemovePlanRequest>
{
    public RemovePlanRequestValidator(AppDbContext db, ICurrentUser currentUser, IUserDateService dates)
    {
        RuleFor(x => x.Date).MustAsync(async (date, ct) =>
        {
            var user = await db.Users.FindAsync([currentUser.Id], ct);
            
            return user is null || 
                   date >= dates.Today(user);
        }).WithMessage("Past plans cannot be changed.");
    }
}
