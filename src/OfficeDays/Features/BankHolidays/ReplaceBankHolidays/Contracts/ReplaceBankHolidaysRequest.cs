using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;

public sealed record ReplaceBankHolidaysRequest : IRequest
{
    [FromRoute(Name = "countryCode")]
    public string CountryCode { get; init; } = default!;

    [FromRoute(Name = "year")]
    public int Year { get; init; }

    [FromBody]
    public List<BankHolidayItem>? Body { get; init; }
}
