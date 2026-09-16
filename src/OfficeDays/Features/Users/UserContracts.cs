namespace OfficeDays.Features.Users;

public sealed record CreateUserRequest(string? Username, string? Password, string? TimeZoneId, string? CountryCode);
public sealed record UserResponse(Guid Id, string Username, string TimeZoneId,
                                  string CountryCode, string CountryName, bool IsAdmin);
