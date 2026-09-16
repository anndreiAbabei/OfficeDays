namespace OfficeDays.Features.HolidayJurisdictions;

public sealed record CreateHolidayJurisdictionRequest(string? Code, string? Name);
public sealed record UpdateHolidayJurisdictionRequest(string? Name);
public sealed record HolidayJurisdictionResponse(string Code, string Name);
