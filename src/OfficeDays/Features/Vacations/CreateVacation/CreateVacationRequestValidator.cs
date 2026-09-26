using FluentValidation;
using OfficeDays.Features.Vacations.CreateVacation.Contracts;

namespace OfficeDays.Features.Vacations.CreateVacation;

public sealed class CreateVacationRequestValidator : AbstractValidator<CreateVacationRequest>
{
    public CreateVacationRequestValidator()
    {
        RuleFor(input => input.Body).Must(body => body.To >= body.From)
            .WithMessage("Vacation end date cannot precede its start date.")
            .OverridePropertyName("to");
    }
}
