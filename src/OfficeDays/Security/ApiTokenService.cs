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
        
        return string.Format(TokenFormat, WebEncoders.Base64UrlEncode(bytes));
    }

    public string Hash(string rawToken) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
