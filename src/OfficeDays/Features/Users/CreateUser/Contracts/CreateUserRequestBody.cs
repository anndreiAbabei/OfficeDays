namespace OfficeDays.Features.Users.CreateUser.Contracts;

public sealed record CreateUserRequestBody
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string TimeZoneId { get; init; }
    public required string CountryCode { get; init; }
    public string? Email { get; init; }
    public int? RequiredOfficePercentage { get; init; } = 50;
}
