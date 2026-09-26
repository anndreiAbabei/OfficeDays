using FluentValidation;
using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.BankHolidays.GetBankHolidays;

public sealed class GetBankHolidaysRequestValidator : AbstractValidator<GetBankHolidaysRequest>
{
    public GetBankHolidaysRequestValidator()
    {
        RuleFor(input => input.CountryCode).Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required.")
            .OverridePropertyName("countryCode");
        RuleFor(input => input.Year).InclusiveBetween(1, 9999)
            .WithMessage("Year must be between 1 and 9999.").OverridePropertyName("year");
    }
}
