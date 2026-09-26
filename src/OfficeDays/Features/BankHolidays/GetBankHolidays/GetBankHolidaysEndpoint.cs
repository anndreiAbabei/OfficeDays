using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays;

public sealed class GetBankHolidaysEndpoint(IRequestExecutor executor) : IBankHolidaysEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("{countryCode}/{year:int}", executor.Execute<GetBankHolidaysRequest>).AllowAnonymous();
    }
}
