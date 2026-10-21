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
        RuleFor(x => x.RequiredOfficePercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.Mode)
            .Must(mode => !mode.HasValue || Enum.IsDefined(mode.Value))
            .WithMessage("Choose Percentage or DaysPerWeek.");
        RuleFor(x => x.Value)
            .NotNull().When(x => x.Mode is not null);
        RuleFor(x => x.Mode)
            .NotNull().When(x => x.Value.HasValue);
        RuleFor(x => x.Value)
            .InclusiveBetween(0, 100).When(x => x.Mode == AttendanceRequirementMode.Percentage);
        RuleFor(x => x.Value)
            .InclusiveBetween(0, 5).When(x => x.Mode == AttendanceRequirementMode.DaysPerWeek);
        RuleFor(x => x.RequiredOfficePercentage)
            .Null().When(x => x.Mode is not null || x.Value.HasValue)
            .WithMessage("Provide mode and value, or requiredOfficePercentage, but not both.");
        RuleFor(x => x.RequiredOfficePercentage)
            .NotNull().When(x => x.Mode is null && !x.Value.HasValue);

        RuleFor(x => x.StartingFrom)
            .Must(date => date.Day == 1)
            .WithMessage("Choose the first day of an effective month.");
    }
}
