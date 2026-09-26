using FluentValidation;
using OfficeDays.Features.Users.UpdateUser.Contracts;

namespace OfficeDays.Features.Users.UpdateUser;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(input => input.Body.Email).Must(UserFields.IsValidEmail)
            .WithMessage("Enter a valid email address of at most 250 characters.")
            .OverridePropertyName("email");
        RuleFor(input => input.Body.RequiredOfficePercentage).InclusiveBetween(0, 100)
            .WithMessage("Required office percentage must be a whole number between 0 and 100.")
            .OverridePropertyName("requiredOfficePercentage");
    }
}
