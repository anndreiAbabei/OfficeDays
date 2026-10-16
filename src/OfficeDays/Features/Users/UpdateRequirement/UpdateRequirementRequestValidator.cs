using FluentValidation;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementRequestValidator : AbstractValidator<UpdateRequirementRequest>
{
    public UpdateRequirementRequestValidator(IValidator<UpdateRequirementRequestBody> bodyValidator)
    {
        RuleFor(x => x.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class UpdateRequirementRequestBodyValidator : AbstractValidator<UpdateRequirementRequestBody>
{
    public UpdateRequirementRequestBodyValidator()
    {
        RuleFor(x => x.RequiredOfficePercentage)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.StartingFrom)
            .Must(date => date.Day == 1)
            .WithMessage("Choose the first day of an effective month.");
    }
}
