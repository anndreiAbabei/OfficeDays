using FluentValidation;
using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.Users.CreateUser;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(IUserDateService dates)
    {
        RuleFor(input => input.Body.Username).Must(username =>
            !string.IsNullOrWhiteSpace(username) && username.Trim().Length is >= 3 and <= 100)
            .WithMessage("Username must contain between 3 and 100 characters.")
            .OverridePropertyName("username");
        RuleFor(input => input.Body.Password).Must(password => !string.IsNullOrEmpty(password) && password.Length >= 12)
            .WithMessage("Password must contain at least 12 characters.")
            .OverridePropertyName("password");
        RuleFor(input => input.Body.TimeZoneId).Must(id => !string.IsNullOrWhiteSpace(id) && dates.IsValidTimeZone(id))
            .WithMessage("A valid IANA timezone is required, for example Europe/Bucharest.")
            .OverridePropertyName("timeZoneId");
        RuleFor(input => input.Body.CountryCode).Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required, for example RO, GB-NIR, or US.")
            .OverridePropertyName("countryCode");
        RuleFor(input => input.Body.Email).Must(UserFields.IsValidEmail)
            .WithMessage("Enter a valid email address of at most 250 characters.")
            .OverridePropertyName("email");
        RuleFor(input => input.Body.RequiredOfficePercentage).InclusiveBetween(0, 100)
            .WithMessage("Required office percentage must be a whole number between 0 and 100.")
            .OverridePropertyName("requiredOfficePercentage");
    }
}
