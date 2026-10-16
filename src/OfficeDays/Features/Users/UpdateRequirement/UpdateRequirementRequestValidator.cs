using FluentValidation;
using OfficeDays.Features.Users.Requirements;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementRequestValidator : AbstractValidator<UpdateRequirementRequest>
{
    public UpdateRequirementRequestValidator(IValidator<RequirementBody> bodyValidator)
    {
        RuleFor(x => x.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}
