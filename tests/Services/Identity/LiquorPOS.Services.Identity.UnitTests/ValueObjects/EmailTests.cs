using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.ValueObjects;

namespace LiquorPOS.Services.Identity.UnitTests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        var result = Email.Create("user@example.com");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_WithValidEmail_ShouldNormalizeToLowercase()
    {
        var result = Email.Create("User@Example.COM");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldFail()
    {
        var result = Email.Create("");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespaceEmail_ShouldFail()
    {
        var result = Email.Create("   ");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email cannot be empty");
    }

    [Fact]
    public void Create_WithNullEmail_ShouldFail()
    {
        var result = Email.Create(null!);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email cannot be empty");
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid@.com")]
    [InlineData("invalid @email.com")]
    public void Create_WithInvalidEmailFormat_ShouldFail(string email)
    {
        var result = Email.Create(email);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email format is invalid");
    }

    [Fact]
    public void Create_WithEmailExceeding254Characters_ShouldFail()
    {
        var longEmail = new string('a', 250) + "@test.com";
        var result = Email.Create(longEmail);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email cannot exceed 254 characters");
    }

    [Fact]
    public void CreateOrThrow_WithValidEmail_ShouldReturnEmail()
    {
        var email = Email.CreateOrThrow("user@example.com");

        email.Should().NotBeNull();
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void CreateOrThrow_WithInvalidEmail_ShouldThrowArgumentException()
    {
        var action = () => Email.CreateOrThrow("");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        var email1 = Email.CreateOrThrow("user@example.com");
        var email2 = Email.CreateOrThrow("user@example.com");

        email1.Equals(email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        var email1 = Email.CreateOrThrow("user1@example.com");
        var email2 = Email.CreateOrThrow("user2@example.com");

        email1.Equals(email2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldBeFalse()
    {
        var email = Email.CreateOrThrow("user@example.com");

        email.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ShouldBeTrue()
    {
        var email1 = Email.CreateOrThrow("user@example.com");
        var email2 = Email.CreateOrThrow("user@example.com");

        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentValue_ShouldBeFalse()
    {
        var email1 = Email.CreateOrThrow("user1@example.com");
        var email2 = Email.CreateOrThrow("user2@example.com");

        (email1 == email2).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ShouldBeTrue()
    {
        var email1 = Email.CreateOrThrow("user1@example.com");
        var email2 = Email.CreateOrThrow("user2@example.com");

        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        var email1 = Email.CreateOrThrow("user@example.com");
        var email2 = Email.CreateOrThrow("user@example.com");

        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ShouldReturnDifferentHashCode()
    {
        var email1 = Email.CreateOrThrow("user1@example.com");
        var email2 = Email.CreateOrThrow("user2@example.com");

        email1.GetHashCode().Should().NotBe(email2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        var email = Email.CreateOrThrow("user@example.com");

        email.ToString().Should().Be("user@example.com");
    }

    [Fact]
    public void Create_WithLeadingAndTrailingWhitespace_ShouldTrim()
    {
        var result = Email.Create("  user@example.com  ");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be("user@example.com");
    }
}
