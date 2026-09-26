using FluentValidation;
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
    public CreateUserRequestBodyValidator(IUserDateService dates, IUserService userService)
    {
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
            .NotEmpty()
            .EmailAddress();
            
        When(input => input.RequiredOfficePercentage.HasValue, () =>
            {
                RuleFor(input => input.RequiredOfficePercentage)
                    .InclusiveBetween(0, 100);
            });
    }
}