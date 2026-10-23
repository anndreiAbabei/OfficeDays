using FluentValidation;
using OfficeDays.Domain;
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
        RuleFor(x => x.RequirementMode)
            .Must(mode => !mode.HasValue || Enum.IsDefined(mode.Value))
            .WithMessage("Choose Percentage or DaysPerWeek.");
        RuleFor(x => x.RequirementValue)
            .NotNull();
        RuleFor(x => x.RequirementMode)
            .NotNull();
        RuleFor(x => x.RequirementValue)
            .InclusiveBetween(0, 100).When(x => x.RequirementMode == AttendanceRequirementMode.Percentage);
        RuleFor(x => x.RequirementValue)
            .InclusiveBetween(0, 5).When(x => x.RequirementMode == AttendanceRequirementMode.DaysPerWeek);

        RuleFor(x => x.StartingFrom)
            .Must(date => date.Day == 1)
            .WithMessage("Choose the first day of an effective month.");
    }
}
