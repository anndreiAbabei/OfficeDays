namespace OfficeDays.Features.Users;

public sealed record CreateUserRequest(string? Username, string? Password, string? TimeZoneId, string? CountryCode, string? Email = null, int RequiredOfficePercentage = 50);
public sealed record UpdateUserRequest(string? Email, int? RequiredOfficePercentage = null);
public sealed record UserResponse(Guid Id, string Username, string TimeZoneId,
                                  string CountryCode, string CountryName, bool IsAdmin, string? Email, int RequiredOfficePercentage);
