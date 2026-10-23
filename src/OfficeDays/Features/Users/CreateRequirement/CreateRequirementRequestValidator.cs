using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateRequirement.Contracts;

namespace OfficeDays.Features.Users.CreateRequirement;

public sealed class CreateRequirementRequestValidator : AbstractValidator<CreateRequirementRequest>
{
    public CreateRequirementRequestValidator(IValidator<CreateRequirementRequestBody> bodyValidator)
    {
        RuleFor(x => x.Body)
            .NotNull()
            .SetValidator(bodyValidator);
        
        RuleFor(x => x.Body.StartingFrom)
            .GreaterThan(DateOnly.MinValue)
            .When([SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")] (x) => x.Body is not null)
            .WithMessage("The initial requirement already exists. Choose an effective month.");
    }
}

public sealed class CreateRequirementRequestBodyValidator : AbstractValidator<CreateRequirementRequestBody>
{
    public CreateRequirementRequestBodyValidator()
    {
        const int maxDaysPerWeek = 5;
        
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
            .InclusiveBetween(0, maxDaysPerWeek).When(x => x.RequirementMode == AttendanceRequirementMode.DaysPerWeek);

        RuleFor(x => x.StartingFrom)
            .Must(date => date.Day == 1)
            .WithMessage("Choose the first day of an effective month.");
    }
}
