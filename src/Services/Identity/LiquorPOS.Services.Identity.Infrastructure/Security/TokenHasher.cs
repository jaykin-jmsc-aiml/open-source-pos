using System.Security.Cryptography;
using System.Text;

namespace LiquorPOS.Services.Identity.Infrastructure.Security;

public static class TokenHasher
{
    public static string Hash(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }

    public static bool Verify(string token, string hash)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(hash))
            return false;

        var tokenHash = Hash(token);
        return tokenHash == hash;
    }
}
