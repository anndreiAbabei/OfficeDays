using FluentValidation;
using OfficeDays.Features.Status.GetStatus.Contracts;

namespace OfficeDays.Features.Status.GetStatus;

public sealed class GetStatusRequestValidator : AbstractValidator<GetStatusRequest>
{
    public GetStatusRequestValidator()
    {
        When(r => r.Year.HasValue, () =>
        {
            RuleFor(r => r.Year)
                .GreaterThan(0)
                .LessThan(9999);

            RuleFor(r => r.Month)
                .NotNull();
        });
        
        When(r => r.Month.HasValue, () =>
        {
            RuleFor(r => r.Month)
                .GreaterThan(0)
                .LessThan(9999);

            RuleFor(r => r.Year)
                .NotNull();
        });
    }
}
