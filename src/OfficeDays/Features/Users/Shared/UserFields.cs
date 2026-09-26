using System.ComponentModel.DataAnnotations;

namespace OfficeDays.Features.Users;

internal static class UserFields
{
    public static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim();

    public static bool IsValidEmail(string? email)
    {
        var normalized = NormalizeEmail(email);
        return normalized is null || (normalized.Length <= 250 && new EmailAddressAttribute().IsValid(normalized));
    }
}
