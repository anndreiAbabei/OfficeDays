using FluentValidation;
using OfficeDays.Features.Attendance.RecordDate.Contracts;

namespace OfficeDays.Features.Attendance.RecordDate;

public sealed class RecordDateRequestValidator : AbstractValidator<RecordDateRequest>
{
    public RecordDateRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(r => r.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime));
    }
}
