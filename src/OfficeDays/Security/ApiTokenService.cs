using System.Buffers;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace OfficeDays.Security;

public sealed class ApiTokenService : IApiTokenService
{
    private const string TokenPrefix = "odt_";

    public string Generate()
    {
        const int tokenLength = 32;
        var encodedLength = Base64Url.GetEncodedLength(tokenLength); 

        return string.Create(TokenPrefix.Length + encodedLength, state: 0, static (destination, _) =>
        {
            TokenPrefix.AsSpan().CopyTo(destination);

            Span<byte> randomBytes = stackalloc byte[tokenLength];
            RandomNumberGenerator.Fill(randomBytes);

            Base64Url.EncodeToChars(randomBytes, destination[TokenPrefix.Length..]);
        });
    }

    public string Hash(string rawToken)
    {
        const int stackAllocThreshold = 512;

        var charSpan = rawToken.AsSpan();
        var maxByteCount = Encoding.UTF8.GetByteCount(charSpan);

        byte[]? rentedBuffer = null;
        var byteBuffer = maxByteCount <= stackAllocThreshold // if token is too big to fit on the stack will use an array pool array
                             ? stackalloc byte[stackAllocThreshold]
                             : rentedBuffer = ArrayPool<byte>.Shared.Rent(maxByteCount);

        try
        {
            var bytesWritten = Encoding.UTF8.GetBytes(charSpan, byteBuffer);
            ReadOnlySpan<byte> utf8Bytes = byteBuffer[..bytesWritten];

            Span<byte> hashBuffer = stackalloc byte[SHA256.HashSizeInBytes];
            SHA256.HashData(utf8Bytes, hashBuffer);

            return Convert.ToHexStringLower(hashBuffer);
        }
        finally
        {
            if (rentedBuffer != null)
                ArrayPool<byte>.Shared.Return(rentedBuffer, clearArray: true);
        }
    }
}
