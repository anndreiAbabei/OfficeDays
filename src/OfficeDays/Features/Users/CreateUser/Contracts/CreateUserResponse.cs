namespace OfficeDays.Features.Users.CreateUser.Contracts;

public sealed record CreateUserResponse(Guid Id, string Username, string TimeZoneId,
                                  string CountryCode, string CountryName, bool IsAdmin, string? Email, int RequiredOfficePercentage);
