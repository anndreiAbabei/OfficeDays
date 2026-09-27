using FluentValidation;
using OfficeDays.Features.Vacations.GetVacations.Contracts;

namespace OfficeDays.Features.Vacations.GetVacations;

public sealed class GetVacationsRequestValidator : AbstractValidator<GetVacationsRequest>
{
    public GetVacationsRequestValidator()
    {
        When(req => req.Year.HasValue, () =>
        {
            RuleFor(req => req.Year)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(9999);

            RuleFor(req => req.Month)
                .NotNull();
        });
        
        When(req => req.Month.HasValue, () =>
        {
            RuleFor(req => req.Month)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(12);

            RuleFor(req => req.Year)
                .NotNull();
        });
    }
}
