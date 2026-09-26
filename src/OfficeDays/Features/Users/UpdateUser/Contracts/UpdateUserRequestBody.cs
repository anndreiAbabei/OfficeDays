namespace OfficeDays.Features.Users.UpdateUser.Contracts;

public sealed record UpdateUserRequestBody(string? Email, int? RequiredOfficePercentage = null);
