using OfficeDays.Features.Attendance;
using OfficeDays.Features.Authentication;
using OfficeDays.Features.BankHolidays;
using OfficeDays.Features.HolidayJurisdictions;
using OfficeDays.Features.Status;
using OfficeDays.Features.Tokens;
using OfficeDays.Features.Users;
using OfficeDays.Features.Vacations;

namespace OfficeDays.Endpoints;

public static class ApiEndpointMappings
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");
        AuthenticationEndpoints.Map(api);
        UserEndpoints.Map(api);
        TokenEndpoints.Map(api);
        AttendanceEndpoints.Map(api);
        VacationEndpoints.Map(api);
        BankHolidayEndpoints.Map(api);
        HolidayJurisdictionEndpoints.Map(api);
        StatusEndpoints.Map(api);
    }
}
