using System.Text.RegularExpressions;
using LiquorPOS.BuildingBlocks.Results;

namespace LiquorPOS.Services.Identity.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    private const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>("Email cannot be empty");

        var trimmed = email.Trim().ToLowerInvariant();

        if (trimmed.Length > 254)
            return Result.Failure<Email>("Email cannot exceed 254 characters");

        if (!Regex.IsMatch(trimmed, EmailPattern))
            return Result.Failure<Email>("Email format is invalid");

        return Result.Success<Email>(new Email(trimmed));
    }

    public static Email CreateOrThrow(string email)
    {
        var result = Create(email);
        if (!result.IsSuccess)
            throw new ArgumentException(result.Error);
        return result.Value!;
    }

    public override bool Equals(object? obj) => Equals(obj as Email);

    public bool Equals(Email? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value, StringComparison.Ordinal);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(Email? left, Email? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Email? left, Email? right) => !(left == right);
}
