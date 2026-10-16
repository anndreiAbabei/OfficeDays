using FluentValidation;

namespace OfficeDays.Features.Users.Requirements;

public sealed record RequirementBody
{
    public required DateOnly StartingFrom { get; init; }
    public required int RequiredOfficePercentage { get; init; }
}

public sealed class RequirementBodyValidator : AbstractValidator<RequirementBody>
{
    public RequirementBodyValidator()
    {
        RuleFor(x => x.RequiredOfficePercentage)
            .InclusiveBetween(0, 100);
        
        RuleFor(x => x.StartingFrom)
            .Must(date => date.Day == 1)
            .WithMessage("Choose the first day of an effective month.");
    }
}
