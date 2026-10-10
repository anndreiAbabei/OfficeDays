using FluentValidation;
using OfficeDays.Features.Planning.GetPlanning.Contracts;

namespace OfficeDays.Features.Planning.GetPlanning;

public sealed class GetPlanningRequestValidator : AbstractValidator<GetPlanningRequest>
{
    public GetPlanningRequestValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(1, 9999);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
