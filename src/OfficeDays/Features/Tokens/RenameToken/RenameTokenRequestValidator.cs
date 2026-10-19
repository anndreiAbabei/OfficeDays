using FluentValidation;
using OfficeDays.Features.Tokens.RenameToken.Contracts;

namespace OfficeDays.Features.Tokens.RenameToken;

public sealed class RenameTokenRequestValidator : AbstractValidator<RenameTokenRequest>
{
    public RenameTokenRequestValidator(IValidator<RenameTokenRequestBody> bodyValidator)
    {
        RuleFor(input => input.Body).NotNull().SetValidator(bodyValidator);
    }
}

public sealed class RenameTokenRequestBodyValidator : AbstractValidator<RenameTokenRequestBody>
{
    public RenameTokenRequestBodyValidator()
    {
        RuleFor(body => body.Name).NotEmpty().MaximumLength(100);
    }
}
