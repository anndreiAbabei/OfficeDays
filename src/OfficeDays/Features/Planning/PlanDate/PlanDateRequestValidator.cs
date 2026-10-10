using FluentValidation;
using OfficeDays.Data;
using OfficeDays.Features.Planning.PlanDate.Contracts;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Planning.PlanDate;

public sealed class PlanDateRequestValidator : AbstractValidator<PlanDateRequest>
{
    public PlanDateRequestValidator(AppDbContext db, ICurrentUser currentUser, IUserDateService dates)
    {
        RuleFor(x => x.Date).MustAsync(async (date, ct) =>
        {
            var user = await db.Users.FindAsync([currentUser.Id], ct);
            
            return user is null || 
                   date >= dates.Today(user);
        }).WithMessage("Past plans cannot be changed.");
    }
}
