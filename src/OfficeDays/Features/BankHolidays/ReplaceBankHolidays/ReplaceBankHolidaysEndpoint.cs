using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

public sealed class ReplaceBankHolidaysEndpoint(IRequestExecutor executor) : IBankHolidaysEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("{countryCode}/{year:int}", _executor.Execute<ReplaceBankHolidaysRequest>)
                 .RequireAuthorization("AdminOnly")
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
