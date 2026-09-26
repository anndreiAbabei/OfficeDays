using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

public sealed class ReplaceBankHolidaysEndpoint(IRequestExecutor executor) : IBankHolidaysEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("{countryCode}/{year:int}", executor.Execute<ReplaceBankHolidaysRequest>).RequireAuthorization("AdminOnly").AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
