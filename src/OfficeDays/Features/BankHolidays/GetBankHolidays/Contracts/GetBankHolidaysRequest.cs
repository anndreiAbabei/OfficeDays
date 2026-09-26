using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;

public sealed record GetBankHolidaysRequest : IRequest
{
    [FromRoute(Name = "countryCode")]
    public required string CountryCode { get; init; }

    [FromRoute(Name = "year")]
    public required int Year { get; init; }
}
