using FluentAssertions;
using FluentValidation.TestHelper;
using LiquorPOS.Services.Identity.Application.Commands.RefreshToken;

namespace LiquorPOS.Services.Identity.UnitTests.Commands;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new RefreshTokenCommand("valid_refresh_token_123456789012345678901234567890");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyRefreshToken_ShouldFail()
    {
        var command = new RefreshTokenCommand("");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Refresh token is required");
    }

    [Fact]
    public void Validate_WithNullRefreshToken_ShouldFail()
    {
        var command = new RefreshTokenCommand(null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void Validate_WithShortRefreshToken_ShouldFail()
    {
        var command = new RefreshTokenCommand("short_token");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Refresh token must be at least 32 characters long");
    }

    [Fact]
    public void Validate_WithWhitespaceOnlyRefreshToken_ShouldFail()
    {
        var command = new RefreshTokenCommand("   ");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Refresh token is required");
    }
}