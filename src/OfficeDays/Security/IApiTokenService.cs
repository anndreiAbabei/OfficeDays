namespace OfficeDays.Security;

public interface IApiTokenService
{
    string Generate();
    string Hash(string rawToken);
}
