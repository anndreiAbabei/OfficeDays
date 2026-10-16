using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using OfficeDays.Features.Users.Requirements;
using OfficeDays.Features.Users.CreateRequirement.Contracts;

namespace OfficeDays.Features.Users.CreateRequirement;

public sealed class CreateRequirementRequestValidator : AbstractValidator<CreateRequirementRequest>
{
    public CreateRequirementRequestValidator(IValidator<RequirementBody> bodyValidator)
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
