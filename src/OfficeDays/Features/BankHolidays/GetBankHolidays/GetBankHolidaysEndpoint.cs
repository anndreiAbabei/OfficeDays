using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays;

public sealed class GetBankHolidaysEndpoint(IRequestExecutor executor) : IBankHolidaysEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("{countryCode}/{year:int}", _executor.Execute<GetBankHolidaysRequest>)
                 .AllowAnonymous();
    }
}
