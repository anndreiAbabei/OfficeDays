using FluentValidation;
using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

public sealed class UpdateJurisdictionRequestValidator : AbstractValidator<UpdateJurisdictionRequest>
{
    public UpdateJurisdictionRequestValidator(IValidator<UpdateJurisdictionRequestBody> bodyValidator)
    {
        RuleFor(input => input.Code)
            .Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required, for example RO, GB-NIR, or US.")
            .OverridePropertyName("code");
        
        RuleFor(input => input.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class UpdateJurisdictionRequestBodyValidator : AbstractValidator<UpdateJurisdictionRequestBody>
{
    public UpdateJurisdictionRequestBodyValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}