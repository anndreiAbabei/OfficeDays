using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace OfficeDays.Security;

public static class ApiTokenService
{
    public static string Generate()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        
        return "odt_" + WebEncoders.Base64UrlEncode(bytes);
    }

    public static string Hash(string rawToken) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
