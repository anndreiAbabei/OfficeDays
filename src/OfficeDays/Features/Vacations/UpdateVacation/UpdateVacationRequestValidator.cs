using FluentValidation;
using OfficeDays.Features.Vacations.UpdateVacation.Contracts;

namespace OfficeDays.Features.Vacations.UpdateVacation;

public sealed class UpdateVacationRequestValidator : AbstractValidator<UpdateVacationRequest>
{
    public UpdateVacationRequestValidator(IValidator<UpdateVacationRequestBody> bodyValidator)
    {
        RuleFor(input => input.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class UpdateVacationRequestBodyValidator : AbstractValidator<UpdateVacationRequestBody>
{
    public UpdateVacationRequestBodyValidator()
    {
        RuleFor(input => input.To)
            .GreaterThanOrEqualTo(input => input.From);
    }
}
