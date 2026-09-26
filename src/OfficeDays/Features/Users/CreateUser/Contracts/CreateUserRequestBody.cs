namespace OfficeDays.Features.Users.CreateUser.Contracts;

public sealed record CreateUserRequestBody(string? Username, string? Password, string? TimeZoneId, string? CountryCode, string? Email = null, int RequiredOfficePercentage = 50);
