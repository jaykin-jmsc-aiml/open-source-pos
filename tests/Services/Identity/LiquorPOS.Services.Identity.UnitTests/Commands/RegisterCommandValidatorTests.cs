using FluentAssertions;
using FluentValidation.TestHelper;
using LiquorPOS.Services.Identity.Application.Commands.Register;

namespace LiquorPOS.Services.Identity.UnitTests.Commands;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "John",
            "Doe",
            "SecurePass123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        var command = new RegisterCommand(
            "",
            "John",
            "Doe",
            "SecurePass123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        var command = new RegisterCommand(
            "invalid-email",
            "John",
            "Doe",
            "SecurePass123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyFirstName_ShouldFail()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "",
            "Doe",
            "SecurePass123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_WithEmptyLastName_ShouldFail()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "John",
            "",
            "SecurePass123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Validate_WithWeakPassword_ShouldFail()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "John",
            "Doe",
            "weak");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordMissingUppercase_ShouldFail()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "John",
            "Doe",
            "weakpassword123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordMissingLowercase_ShouldFail()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "John",
            "Doe",
            "WEAKPASSWORD123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordMissingDigit_ShouldFail()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "John",
            "Doe",
            "WeakPassword");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithValidPhoneNumber_ShouldSucceed()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "John",
            "Doe",
            "SecurePass123",
            "+1234567890");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_WithValidRoles_ShouldSucceed()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "John",
            "Doe",
            "SecurePass123",
            null,
            new[] { "Manager", "Cashier" });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Roles);
    }
}
