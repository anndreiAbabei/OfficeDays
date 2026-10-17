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

public sealed class UpdateVacationRequestBodyValidator : VacationDatesValidator<UpdateVacationRequestBody>
{
    public UpdateVacationRequestBodyValidator() : base(input => input.From, input => input.To)
    {
    }
}
