namespace OfficeDays.Features.Authentication.GetCurrentUser.Contracts;

public sealed record GetCurrentUserResponse(Guid Id, string Username, string TimeZoneId,
                                  string CountryCode, string CountryName, bool IsAdmin, string? Email, int RequiredOfficePercentage);
