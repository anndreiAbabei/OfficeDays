using FluentValidation;
using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;
using OfficeDays.Services;
namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

public sealed class ReplaceBankHolidaysRequestValidator : AbstractValidator<ReplaceBankHolidaysRequest>
{
    public ReplaceBankHolidaysRequestValidator(IValidator<BankHolidayItem> bankHolidayItemValidator)
    {
        RuleFor(input => input.CountryCode)
            .Must(code => HolidayJurisdictionCodes.TryNormalize(code, out _))
            .WithMessage("A valid country or subdivision code is required.")
            .OverridePropertyName("countryCode");
        
        RuleFor(input => input.Year)
            .InclusiveBetween(1, 9999)
            .WithMessage("Year must be between 1 and 9999.")
            .OverridePropertyName("year");
        
        RuleFor(input => input.Body)
            .NotNull()
            .NotEmpty()
            .ForEach(bh => bh.NotNull()
                             .SetValidator(bankHolidayItemValidator))
            .Custom((input, context) =>
            {
                if (input.GroupBy(item => item.Date).Any(group => group.Count() > 1))
                    context.AddFailure("date", "Holiday dates must be unique within the replacement payload.");
            });
    }
}

public sealed class BankHolidayItemValidator : AbstractValidator<BankHolidayItem>
{
    public BankHolidayItemValidator()
    {
        RuleFor(item => item.Name)
            .NotEmpty()
            .MaximumLength(150);
    }
}