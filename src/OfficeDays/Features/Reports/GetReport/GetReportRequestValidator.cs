using FluentValidation;
using OfficeDays.Features.Reports.GetReport.Contracts;

namespace OfficeDays.Features.Reports.GetReport;

public sealed class GetReportRequestValidator : AbstractValidator<GetReportRequest>
{
    public GetReportRequestValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(1, 9998)
            .When(x => x.Year.HasValue);
        
        RuleFor(x => x.StartMonth)
            .InclusiveBetween(1, 12)
            .When(x => x.StartMonth.HasValue);
        
        RuleFor(x => x.EndMonth)
            .InclusiveBetween(1, 12)
            .When(x => x.EndMonth.HasValue);
        
        RuleFor(x => x)
            .Must(x => (x.EndMonth ?? 12) >= (x.StartMonth ?? 1))
            .WithMessage("The ending month must be on or after the starting month.");
    }
}
