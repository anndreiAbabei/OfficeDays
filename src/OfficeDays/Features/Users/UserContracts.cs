namespace OfficeDays.Features.Users;

public sealed record CreateUserRequest(string? Username, string? Password, string? TimeZoneId, string? CountryCode, string? Email = null);
public sealed record UpdateUserRequest(string? Email);
public sealed record UserResponse(Guid Id, string Username, string TimeZoneId,
                                  string CountryCode, string CountryName, bool IsAdmin, string? Email);
