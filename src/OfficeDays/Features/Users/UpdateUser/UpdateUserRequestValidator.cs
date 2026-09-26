using FluentValidation;
using OfficeDays.Features.Users.UpdateUser.Contracts;

namespace OfficeDays.Features.Users.UpdateUser;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(IValidator<UpdateUserRequestBody> bodyValidator)
    {
        RuleFor(input => input.Body)
            .NotNull()
            .SetValidator(bodyValidator);
    }
}

public sealed class UpdateUserRequestBodyValidator : AbstractValidator<UpdateUserRequestBody>
{
    public UpdateUserRequestBodyValidator()
    {
        RuleFor(body => body.Email)
            .NotNull()
            .EmailAddress();

        When(body => body.RequiredOfficePercentage.HasValue, () =>
        {
            RuleFor(body => body.RequiredOfficePercentage)
                .InclusiveBetween(0, 100);
        });
    }
}