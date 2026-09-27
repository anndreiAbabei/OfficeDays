using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;

public sealed record ReplaceBankHolidaysRequest : IRequest
{
    [FromRoute(Name = "countryCode")]
    public required string CountryCode { get; init; }

    [FromRoute(Name = "year")]
    public required int Year { get; init; }

    [FromBody]
    public required List<BankHolidayItem> Body { get; init; }
}
