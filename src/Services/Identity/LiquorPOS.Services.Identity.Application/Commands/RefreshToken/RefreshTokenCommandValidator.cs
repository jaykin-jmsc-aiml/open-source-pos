using FluentValidation;

namespace LiquorPOS.Services.Identity.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MinimumLength(32)
            .WithMessage("Refresh token must be at least 32 characters long");
    }
}