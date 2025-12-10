using System.Text.RegularExpressions;
using LiquorPOS.BuildingBlocks.Results;

namespace LiquorPOS.Services.Identity.Domain.ValueObjects;

public sealed class PhoneNumber : IEquatable<PhoneNumber>
{
    private const string PhonePattern = @"^[\d\s\-\+\(\)]{10,}$";

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Failure<PhoneNumber>("Phone number cannot be empty");

        var normalized = Regex.Replace(phoneNumber, @"\s", "").Trim();

        if (normalized.Length < 10)
            return Result.Failure<PhoneNumber>("Phone number must have at least 10 digits");

        if (normalized.Length > 15)
            return Result.Failure<PhoneNumber>("Phone number cannot exceed 15 characters");

        if (!Regex.IsMatch(normalized, PhonePattern))
            return Result.Failure<PhoneNumber>("Phone number format is invalid");

        return Result.Success<PhoneNumber>(new PhoneNumber(normalized));
    }

    public static PhoneNumber CreateOrThrow(string phoneNumber)
    {
        var result = Create(phoneNumber);
        if (!result.IsSuccess)
            throw new ArgumentException(result.Error);
        return result.Value!;
    }

    public override bool Equals(object? obj) => Equals(obj as PhoneNumber);

    public bool Equals(PhoneNumber? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value, StringComparison.Ordinal);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(PhoneNumber? left, PhoneNumber? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !(left == right);
}
