using FluentValidation;
using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;

public sealed class DeleteJurisdictionRequestValidator : AbstractValidator<DeleteJurisdictionRequest>
{
    public DeleteJurisdictionRequestValidator()
    {
        RuleFor(input => input.Code)
            .Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required, for example RO, GB-NIR, or US.")
            .OverridePropertyName("code");
    }
}
