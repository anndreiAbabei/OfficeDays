using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;

public sealed record GetBankHolidaysRequest : IRequest
{
    [FromRoute(Name = "countryCode")]
    public string CountryCode { get; init; } = default!;

    [FromRoute(Name = "year")]
    public int Year { get; init; }
}
