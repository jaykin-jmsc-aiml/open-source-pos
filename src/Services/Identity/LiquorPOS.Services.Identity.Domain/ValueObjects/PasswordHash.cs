using System.Security.Cryptography;
using System.Text;
using LiquorPOS.BuildingBlocks.Results;

namespace LiquorPOS.Services.Identity.Domain.ValueObjects;

public sealed class PasswordHash : IEquatable<PasswordHash>
{
    public string Value { get; }
    public string Salt { get; }

    private PasswordHash(string value, string salt)
    {
        Value = value;
        Salt = salt;
    }

    public static Result<PasswordHash> Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<PasswordHash>("Password cannot be empty");

        if (password.Length < 8)
            return Result.Failure<PasswordHash>("Password must be at least 8 characters long");

        if (password.Length > 128)
            return Result.Failure<PasswordHash>("Password cannot exceed 128 characters");

        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);

        return Result.Success<PasswordHash>(new PasswordHash(hash, salt));
    }

    public static PasswordHash CreateOrThrow(string password)
    {
        var result = Create(password);
        if (!result.IsSuccess)
            throw new ArgumentException(result.Error);
        return result.Value!;
    }

    public static PasswordHash FromHashAndSalt(string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty", nameof(hash));
        if (string.IsNullOrWhiteSpace(salt))
            throw new ArgumentException("Salt cannot be empty", nameof(salt));

        return new PasswordHash(hash, salt);
    }

    public bool Verify(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var hash = HashPassword(password, Salt);
        return hash.Equals(Value, StringComparison.Ordinal);
    }

    private static string GenerateSalt()
    {
        var buffer = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }
        return Convert.ToBase64String(buffer);
    }

    private static string HashPassword(string password, string salt)
    {
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(20);
        return Convert.ToBase64String(hash);
    }

    public override bool Equals(object? obj) => Equals(obj as PasswordHash);

    public bool Equals(PasswordHash? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value, StringComparison.Ordinal) &&
               Salt.Equals(other.Salt, StringComparison.Ordinal);
    }

    public override int GetHashCode() => HashCode.Combine(Value, Salt);

    public override string ToString() => "***";

    public static bool operator ==(PasswordHash? left, PasswordHash? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(PasswordHash? left, PasswordHash? right) => !(left == right);
}
