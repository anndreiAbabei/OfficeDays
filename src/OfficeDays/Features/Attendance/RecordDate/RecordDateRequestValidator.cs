using FluentValidation;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Attendance.RecordDate.Contracts;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Attendance.RecordDate;

public sealed class RecordDateRequestValidator : AbstractValidator<RecordDateRequest>
{
    public RecordDateRequestValidator(TimeProvider timeProvider,
                                      ICurrentUser currentUser,
                                      AppDbContext dbContext,
                                      IUserDateService userDateService)
    {
        RuleFor(r => r.Date)
            .MustAsync(async (date, ct) =>
            {
                var userId = currentUser.Id;
                var user = await dbContext.FindAsync<User>(keyValues: [userId], cancellationToken: ct);

                if(user == null)
                    return date <= DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);

                return date <= userDateService.Today(user);
            });
    }
}
