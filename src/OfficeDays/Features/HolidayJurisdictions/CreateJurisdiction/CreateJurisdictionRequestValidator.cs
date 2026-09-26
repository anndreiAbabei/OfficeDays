using FluentValidation;
using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

public sealed class CreateJurisdictionRequestValidator : AbstractValidator<CreateJurisdictionRequest>
{
    public CreateJurisdictionRequestValidator(IValidator<CreateJurisdictionRequestBody> bodyValidator)
    {
        RuleFor(input => input.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class CreateJurisdictionRequestBodyValidator : AbstractValidator<CreateJurisdictionRequestBody>
{
    public CreateJurisdictionRequestBodyValidator()
    {
        RuleFor(input => input.Code)
            .Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required, for example RO, GB-NIR, or US.")
            .OverridePropertyName("code");
        
        RuleFor(input => input.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}