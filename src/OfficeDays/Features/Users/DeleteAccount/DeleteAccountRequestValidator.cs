using FluentValidation;
using OfficeDays.Features.Users.DeleteAccount.Contracts;

namespace OfficeDays.Features.Users.DeleteAccount;

public sealed class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator(IValidator<DeleteAccountRequestBody> bodyValidator)
    {
        RuleFor(input => input.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class DeleteAccountRequestBodyValidator : AbstractValidator<DeleteAccountRequestBody>
{
    public DeleteAccountRequestBodyValidator()
    {
        RuleFor(body => body.Password)
            .NotEmpty();
    }
}
