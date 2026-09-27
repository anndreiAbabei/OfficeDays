using FluentValidation;
using OfficeDays.Features.Tokens.CreateToken.Contracts;

namespace OfficeDays.Features.Tokens.CreateToken;

public sealed class CreateTokenRequestValidator : AbstractValidator<CreateTokenRequest>
{
    public CreateTokenRequestValidator(IValidator<CreateTokenRequestBody> bodyValidator)
    {
        RuleFor(input => input.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class CreateTokenRequestBodyValidator : AbstractValidator<CreateTokenRequestBody>
{
    public CreateTokenRequestBodyValidator()
    {
        RuleFor(b => b.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}