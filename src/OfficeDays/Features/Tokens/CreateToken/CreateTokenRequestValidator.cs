using FluentValidation;
using OfficeDays.Features.Tokens.CreateToken.Contracts;

namespace OfficeDays.Features.Tokens.CreateToken;

public sealed class CreateTokenRequestValidator : AbstractValidator<CreateTokenRequest>
{
    public CreateTokenRequestValidator()
    {
        RuleFor(input => input.Body.Name).Must(name =>
            !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 100)
            .WithMessage("Token name is required and must not exceed 100 characters.")
            .OverridePropertyName("name");
    }
}
