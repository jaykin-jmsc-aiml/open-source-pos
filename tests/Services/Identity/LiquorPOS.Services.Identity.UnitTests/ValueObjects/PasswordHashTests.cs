using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.ValueObjects;

namespace LiquorPOS.Services.Identity.UnitTests.ValueObjects;

public class PasswordHashTests
{
    [Fact]
    public void Create_WithValidPassword_ShouldSucceed()
    {
        var result = PasswordHash.Create("ValidPassword123!");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Value.Should().NotBeEmpty();
        result.Value!.Salt.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithEmptyPassword_ShouldFail()
    {
        var result = PasswordHash.Create("");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Password cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespacePassword_ShouldFail()
    {
        var result = PasswordHash.Create("   ");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Password cannot be empty");
    }

    [Fact]
    public void Create_WithNullPassword_ShouldFail()
    {
        var result = PasswordHash.Create(null!);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Password cannot be empty");
    }

    [Fact]
    public void Create_WithPasswordLessThan8Characters_ShouldFail()
    {
        var result = PasswordHash.Create("short");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Password must be at least 8 characters long");
    }

    [Fact]
    public void Create_WithPasswordExceeding128Characters_ShouldFail()
    {
        var longPassword = new string('a', 129);
        var result = PasswordHash.Create(longPassword);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Password cannot exceed 128 characters");
    }

    [Fact]
    public void CreateOrThrow_WithValidPassword_ShouldReturnPasswordHash()
    {
        var passwordHash = PasswordHash.CreateOrThrow("ValidPassword123!");

        passwordHash.Should().NotBeNull();
        passwordHash.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateOrThrow_WithInvalidPassword_ShouldThrowArgumentException()
    {
        var action = () => PasswordHash.CreateOrThrow("short");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        var passwordHash = PasswordHash.CreateOrThrow("ValidPassword123!");

        passwordHash.Verify("ValidPassword123!").Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        var passwordHash = PasswordHash.CreateOrThrow("ValidPassword123!");

        passwordHash.Verify("WrongPassword456!").Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyPassword_ShouldReturnFalse()
    {
        var passwordHash = PasswordHash.CreateOrThrow("ValidPassword123!");

        passwordHash.Verify("").Should().BeFalse();
    }

    [Fact]
    public void Verify_WithNullPassword_ShouldReturnFalse()
    {
        var passwordHash = PasswordHash.CreateOrThrow("ValidPassword123!");

        passwordHash.Verify(null!).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameHashAndSalt_ShouldBeTrue()
    {
        var password1 = PasswordHash.CreateOrThrow("ValidPassword123!");
        var password2 = PasswordHash.FromHashAndSalt(password1.Value, password1.Salt);

        password1.Equals(password2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentHash_ShouldBeFalse()
    {
        var password1 = PasswordHash.CreateOrThrow("ValidPassword123!");
        var password2 = PasswordHash.CreateOrThrow("DifferentPassword123!");

        password1.Equals(password2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldBeFalse()
    {
        var password = PasswordHash.CreateOrThrow("ValidPassword123!");

        password.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithSameHashAndSalt_ShouldBeTrue()
    {
        var password1 = PasswordHash.CreateOrThrow("ValidPassword123!");
        var password2 = PasswordHash.FromHashAndSalt(password1.Value, password1.Salt);

        (password1 == password2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentHash_ShouldBeFalse()
    {
        var password1 = PasswordHash.CreateOrThrow("ValidPassword123!");
        var password2 = PasswordHash.CreateOrThrow("DifferentPassword123!");

        (password1 == password2).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentHash_ShouldBeTrue()
    {
        var password1 = PasswordHash.CreateOrThrow("ValidPassword123!");
        var password2 = PasswordHash.CreateOrThrow("DifferentPassword123!");

        (password1 != password2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameHashAndSalt_ShouldReturnSameHashCode()
    {
        var password1 = PasswordHash.CreateOrThrow("ValidPassword123!");
        var password2 = PasswordHash.FromHashAndSalt(password1.Value, password1.Salt);

        password1.GetHashCode().Should().Be(password2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentHash_ShouldReturnDifferentHashCode()
    {
        var password1 = PasswordHash.CreateOrThrow("ValidPassword123!");
        var password2 = PasswordHash.CreateOrThrow("DifferentPassword123!");

        password1.GetHashCode().Should().NotBe(password2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnMaskedString()
    {
        var password = PasswordHash.CreateOrThrow("ValidPassword123!");

        password.ToString().Should().Be("***");
    }

    [Fact]
    public void FromHashAndSalt_WithValidValues_ShouldReturnPasswordHash()
    {
        var password1 = PasswordHash.CreateOrThrow("ValidPassword123!");
        var password2 = PasswordHash.FromHashAndSalt(password1.Value, password1.Salt);

        password2.Value.Should().Be(password1.Value);
        password2.Salt.Should().Be(password1.Salt);
    }

    [Fact]
    public void FromHashAndSalt_WithEmptyHash_ShouldThrowArgumentException()
    {
        var action = () => PasswordHash.FromHashAndSalt("", "somesalt");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromHashAndSalt_WithEmptySalt_ShouldThrowArgumentException()
    {
        var action = () => PasswordHash.FromHashAndSalt("somehash", "");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithDifferentPasswords_ShouldGenerateDifferentHashes()
    {
        var password1 = PasswordHash.CreateOrThrow("ValidPassword123!");
        var password2 = PasswordHash.CreateOrThrow("ValidPassword123!");

        password1.Value.Should().NotBe(password2.Value);
    }
}
