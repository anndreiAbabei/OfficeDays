using FluentValidation;
using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

public sealed class CreateJurisdictionRequestValidator : AbstractValidator<CreateJurisdictionRequest>
{
    public CreateJurisdictionRequestValidator()
    {
        RuleFor(input => input.Body.Code).Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required, for example RO, GB-NIR, or US.")
            .OverridePropertyName("code");
        RuleFor(input => input.Body.Name).Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 100)
            .WithMessage("Name must contain between 1 and 100 characters.")
            .OverridePropertyName("name");
    }
}
