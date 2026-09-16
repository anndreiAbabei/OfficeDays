namespace OfficeDays.Features.Users;

public sealed record CreateUserRequest(string? Username, string? Password, string? TimeZoneId);
public sealed record UserResponse(Guid Id, string Username, string TimeZoneId, bool IsAdmin);
