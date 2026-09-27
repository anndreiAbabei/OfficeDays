namespace OfficeDays.Features.Users.CreateUser.Contracts;

public sealed record CreateUserResponse
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string TimeZoneId { get; init; }
    public required string CountryCode { get; init; }
    public required string CountryName { get; init; }
    public required bool IsAdmin { get; init; }
    public required string? Email { get; init; }
    public required int RequiredOfficePercentage { get; init; }
}
