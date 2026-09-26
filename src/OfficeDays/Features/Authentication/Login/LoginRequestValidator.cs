using FluentValidation;
using OfficeDays.Features.Authentication.Login.Contracts;

namespace OfficeDays.Features.Authentication.Login;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(input => input.Body).Must(body =>
            !string.IsNullOrWhiteSpace(body.Username) && !string.IsNullOrEmpty(body.Password))
            .WithMessage("Username and password are required.").OverridePropertyName("request");
    }
}
