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
        RuleFor(body => body.TeamDay)
            .Must(day => day is null or "monday" or "tuesday" or "wednesday" or "thursday" or "friday")
            .WithMessage("Team day must be Monday to Friday (lowercase), or null.");

        RuleFor(body => body.Email)
            .EmailAddress()
            .When(body => !string.IsNullOrWhiteSpace(body.Email));

        When(body => body.RequiredOfficePercentage.HasValue, () =>
        {
            RuleFor(body => body.RequiredOfficePercentage)
                .InclusiveBetween(0, 100);
        });
    }
}