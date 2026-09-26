using FluentValidation;
using OfficeDays.Features.Status.GetStatus.Contracts;

namespace OfficeDays.Features.Status.GetStatus;

public sealed class GetStatusRequestValidator : AbstractValidator<GetStatusRequest>
{
    public GetStatusRequestValidator()
    {
        RuleFor(request => request).Must(request =>
            (!request.Year.HasValue && !request.Month.HasValue) ||
            (request.Year is >= 1 and <= 9999 && request.Month is >= 1 and <= 12))
            .WithMessage("Both year and month must be supplied and valid.")
            .OverridePropertyName("request");
    }
}
