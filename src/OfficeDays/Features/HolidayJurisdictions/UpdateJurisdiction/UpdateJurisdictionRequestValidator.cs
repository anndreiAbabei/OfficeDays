using FluentValidation;
using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

public sealed class UpdateJurisdictionRequestValidator : AbstractValidator<UpdateJurisdictionRequest>
{
    public UpdateJurisdictionRequestValidator()
    {
        RuleFor(input => input.Code).Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required, for example RO, GB-NIR, or US.")
            .OverridePropertyName("code");
        RuleFor(input => input.Body.Name).Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 100)
            .WithMessage("Name must contain between 1 and 100 characters.")
            .OverridePropertyName("name");
    }
}
