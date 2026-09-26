using FluentValidation;
using OfficeDays.Features.Attendance.GetAttendance.Contracts;

namespace OfficeDays.Features.Attendance.GetAttendance;

public sealed class GetAttendanceRequestValidator : AbstractValidator<GetAttendanceRequest>
{
    public GetAttendanceRequestValidator()
    {
        When(s => s.Year.HasValue, () =>
        {
            RuleFor(s => s.Year)
                .GreaterThan(1)
                .LessThanOrEqualTo(9999);
            RuleFor(s => s.Month)
                .NotNull();
        });
        
        When(s => s.Month.HasValue, () =>
        {
            RuleFor(s => s.Month)
                .GreaterThan(1)
                .LessThanOrEqualTo(12);
            RuleFor(s => s.Year)
                .NotNull();
        });
    }
}
