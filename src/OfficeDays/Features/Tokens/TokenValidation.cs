namespace OfficeDays.Features.Tokens;

internal static class TokenValidation
{
    public static string? ValidateName(string? name) =>
        string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100
            ? "Token name is required and must not exceed 100 characters."
            : null;
}
