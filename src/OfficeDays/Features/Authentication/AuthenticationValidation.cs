namespace OfficeDays.Features.Authentication;

internal static class AuthenticationValidation
{
    public static bool HasCredentials(LoginRequest request) =>
        !string.IsNullOrWhiteSpace(request.Username) && !string.IsNullOrEmpty(request.Password);
}
