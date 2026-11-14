using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace OfficeDays.Security;

public sealed class ApiTokenService : IApiTokenService
{
    private const string TokenFormat = "odt_{0}";

    public string Generate()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        var b64 = WebEncoders.Base64UrlEncode(bytes);

        return string.Format(TokenFormat, b64);
    }

    public string Hash(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        
        return Convert.ToHexStringLower(hash);
    }
}
