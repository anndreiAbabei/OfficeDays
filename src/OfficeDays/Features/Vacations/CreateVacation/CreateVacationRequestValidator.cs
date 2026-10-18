using FluentValidation;
using OfficeDays.Features.Vacations.CreateVacation.Contracts;

namespace OfficeDays.Features.Vacations.CreateVacation;

public sealed class CreateVacationRequestValidator : AbstractValidator<CreateVacationRequest>
{
    public CreateVacationRequestValidator(IValidator<CreateVacationRequestBody> bodyValidator)
    {
        RuleFor(input => input.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class CreateVacationRequestBodyValidator : AbstractValidator<CreateVacationRequestBody>
{
    public CreateVacationRequestBodyValidator()
    {
        RuleFor(input => input.To)
            .GreaterThanOrEqualTo(input => input.From);
    }
}