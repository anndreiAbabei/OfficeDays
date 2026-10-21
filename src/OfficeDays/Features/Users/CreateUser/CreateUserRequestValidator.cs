using FluentValidation;
using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.Users.CreateUser;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(IValidator<CreateUserRequestBody> bodyValidator)
    {
        RuleFor(input => input.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class CreateUserRequestBodyValidator : AbstractValidator<CreateUserRequestBody>
{
    public CreateUserRequestBodyValidator(IUserDateService dates)
    {
        RuleFor(x => x.RequiredOfficePercentage)
            .InclusiveBetween(0, 100);
        RuleFor(x => x.Mode)
            .Must(mode => !mode.HasValue || Enum.IsDefined(mode.Value))
            .WithMessage("Choose Percentage or DaysPerWeek.");
        RuleFor(x => x.Value)
            .NotNull().When(x => x.Mode is not null);
        RuleFor(x => x.Mode)
            .NotNull().When(x => x.Value.HasValue);
        RuleFor(x => x.Value)
            .InclusiveBetween(0, 100).When(x => x.Mode == AttendanceRequirementMode.Percentage);
        RuleFor(x => x.Value)
            .InclusiveBetween(0, 5).When(x => x.Mode == AttendanceRequirementMode.DaysPerWeek);
        RuleFor(x => x.RequiredOfficePercentage)
            .Null().When(x => x.Mode is not null || x.Value.HasValue)
            .WithMessage("Provide mode and value, or requiredOfficePercentage, but not both.");

        RuleFor(input => input.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);
            
        RuleFor(input => input.Password)
            .NotEmpty()
            .MinimumLength(12);

        RuleFor(input => input.TimeZoneId)
            .NotEmpty()
            .Must(dates.IsValidTimeZone)
            .WithMessage("A valid IANA timezone is required, for example Europe/Bucharest.");
            
        RuleFor(input => input.CountryCode)
            .NotEmpty()
            .Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required, for example RO, GB-NIR, or US.");
            
        RuleFor(input => input.Email)
            .EmailAddress()
            .When(input => !string.IsNullOrWhiteSpace(input.Email));

    }
}