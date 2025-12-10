using FluentValidation;

namespace LiquorPOS.Services.Identity.Application.Commands.RevokeToken;

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MinimumLength(32)
            .WithMessage("Refresh token must be at least 32 characters long");
    }
}