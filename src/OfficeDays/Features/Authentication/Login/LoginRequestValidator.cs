using FluentValidation;
using OfficeDays.Features.Authentication.Login.Contracts;

namespace OfficeDays.Features.Authentication.Login;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IValidator<LoginRequestBody> bodyValidator)
    {
        RuleFor(input => input.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class LoginRequestBodyValidator : AbstractValidator<LoginRequestBody>
{
    public LoginRequestBodyValidator()
    {
        RuleFor(v => v.Username)
            .NotEmpty();
        
        RuleFor(v => v.Password)
            .NotEmpty();
    }
}