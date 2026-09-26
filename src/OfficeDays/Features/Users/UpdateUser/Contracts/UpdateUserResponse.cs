namespace OfficeDays.Features.Users.UpdateUser.Contracts;

public sealed record UpdateUserResponse(Guid Id, string Username, string TimeZoneId,
                                  string CountryCode, string CountryName, bool IsAdmin, string? Email, int RequiredOfficePercentage);
