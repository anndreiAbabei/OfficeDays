namespace OfficeDays.Services;

public interface IUserService
{
    string NormalizeUsername(string username);
    string? NormalizeEmail(string? email);
}

public sealed class UserService : IUserService
{
    public string NormalizeUsername(string username) => username.Trim().ToUpperInvariant();
    
    public string? NormalizeEmail(string? email) => string.IsNullOrWhiteSpace(email) ? null : email.Trim();
}
