using OfficeDays.Infrastructure;

namespace OfficeDays.Features.BankHolidays;

public interface IBankHolidaysEndpoint : IEndpoint;

public sealed class BankHolidaysEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IBankHolidaysEndpoint>("bank-holidays");
    }
}
