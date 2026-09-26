using FluentValidation;
using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

public sealed class ReplaceBankHolidaysRequestValidator : AbstractValidator<ReplaceBankHolidaysRequest>
{
    public ReplaceBankHolidaysRequestValidator()
    {
        RuleFor(input => input.CountryCode).Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required.")
            .OverridePropertyName("countryCode");
        RuleFor(input => input.Year).InclusiveBetween(1, 9999)
            .WithMessage("Year must be between 1 and 9999.").OverridePropertyName("year");
        RuleFor(input => input).Custom((input, context) =>
        {
            if (input.Body is null)
            {
                context.AddFailure("request", "A holiday array is required.");
                return;
            }
            if (input.Body.Any(item => item is null))
            {
                context.AddFailure("request", "Every holiday must be an object.");
                return;
            }
            if (input.Body.Any(item => item.Date.Year != input.Year))
                context.AddFailure("date", "Every holiday date must belong to the year in the URL.");
            if (input.Body.Any(item => string.IsNullOrWhiteSpace(item.Name) || item.Name.Trim().Length > 150))
                context.AddFailure("name", "Every holiday needs a name of at most 150 characters.");
            if (input.Body.GroupBy(item => item.Date).Any(group => group.Count() > 1))
                context.AddFailure("date", "Holiday dates must be unique within the replacement payload.");
        });
    }
}
