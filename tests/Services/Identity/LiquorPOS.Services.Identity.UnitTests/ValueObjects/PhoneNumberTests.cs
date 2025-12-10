using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.ValueObjects;

namespace LiquorPOS.Services.Identity.UnitTests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("1234567890")]
    [InlineData("+1 (234) 567-8900")]
    [InlineData("123-456-7890")]
    [InlineData("123 456 7890")]
    public void Create_WithValidPhoneNumber_ShouldSucceed(string phoneNumber)
    {
        var result = PhoneNumber.Create(phoneNumber);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithValidPhoneNumber_ShouldNormalizeByRemovingWhitespace()
    {
        var result = PhoneNumber.Create("+1 (234) 567-8900");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().NotContain(" ");
    }

    [Fact]
    public void Create_WithEmptyPhoneNumber_ShouldFail()
    {
        var result = PhoneNumber.Create("");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Phone number cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespacePhoneNumber_ShouldFail()
    {
        var result = PhoneNumber.Create("   ");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Phone number cannot be empty");
    }

    [Fact]
    public void Create_WithNullPhoneNumber_ShouldFail()
    {
        var result = PhoneNumber.Create(null!);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Phone number cannot be empty");
    }

    [Fact]
    public void Create_WithLessThan10Digits_ShouldFail()
    {
        var result = PhoneNumber.Create("123456789");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Phone number must have at least 10 digits");
    }

    [Fact]
    public void Create_WithMoreThan15Characters_ShouldFail()
    {
        var longPhoneNumber = new string('1', 16);
        var result = PhoneNumber.Create(longPhoneNumber);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Phone number cannot exceed 15 characters");
    }

    [Fact]
    public void CreateOrThrow_WithValidPhoneNumber_ShouldReturnPhoneNumber()
    {
        var phoneNumber = PhoneNumber.CreateOrThrow("1234567890");

        phoneNumber.Should().NotBeNull();
    }

    [Fact]
    public void CreateOrThrow_WithInvalidPhoneNumber_ShouldThrowArgumentException()
    {
        var action = () => PhoneNumber.CreateOrThrow("");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        var phone1 = PhoneNumber.CreateOrThrow("1234567890");
        var phone2 = PhoneNumber.CreateOrThrow("1234567890");

        phone1.Equals(phone2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        var phone1 = PhoneNumber.CreateOrThrow("1234567890");
        var phone2 = PhoneNumber.CreateOrThrow("0987654321");

        phone1.Equals(phone2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldBeFalse()
    {
        var phone = PhoneNumber.CreateOrThrow("1234567890");

        phone.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ShouldBeTrue()
    {
        var phone1 = PhoneNumber.CreateOrThrow("1234567890");
        var phone2 = PhoneNumber.CreateOrThrow("1234567890");

        (phone1 == phone2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentValue_ShouldBeFalse()
    {
        var phone1 = PhoneNumber.CreateOrThrow("1234567890");
        var phone2 = PhoneNumber.CreateOrThrow("0987654321");

        (phone1 == phone2).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ShouldBeTrue()
    {
        var phone1 = PhoneNumber.CreateOrThrow("1234567890");
        var phone2 = PhoneNumber.CreateOrThrow("0987654321");

        (phone1 != phone2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        var phone1 = PhoneNumber.CreateOrThrow("1234567890");
        var phone2 = PhoneNumber.CreateOrThrow("1234567890");

        phone1.GetHashCode().Should().Be(phone2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ShouldReturnDifferentHashCode()
    {
        var phone1 = PhoneNumber.CreateOrThrow("1234567890");
        var phone2 = PhoneNumber.CreateOrThrow("0987654321");

        phone1.GetHashCode().Should().NotBe(phone2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnPhoneNumberValue()
    {
        var phone = PhoneNumber.CreateOrThrow("1234567890");

        phone.ToString().Should().Be("1234567890");
    }
}
