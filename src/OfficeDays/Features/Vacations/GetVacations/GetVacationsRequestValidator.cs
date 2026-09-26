using FluentValidation;
using OfficeDays.Features.Vacations.GetVacations.Contracts;

namespace OfficeDays.Features.Vacations.GetVacations;

public sealed class GetVacationsRequestValidator : AbstractValidator<GetVacationsRequest>
{
    public GetVacationsRequestValidator()
    {
        RuleFor(request => request).Must(request =>
            (!request.Year.HasValue && !request.Month.HasValue) ||
            (request.Year is >= 1 and <= 9999 && request.Month is >= 1 and <= 12))
            .WithMessage("Both year and month must be supplied and valid.")
            .OverridePropertyName("request");
    }
}
