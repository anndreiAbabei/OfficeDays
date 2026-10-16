using System.Diagnostics.CodeAnalysis;
using FluentValidation;
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
        RuleFor(x => x.RequiredOfficePercentage)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.StartingFrom)
            .Must(date => date.Day == 1)
            .WithMessage("Choose the first day of an effective month.");
    }
}
