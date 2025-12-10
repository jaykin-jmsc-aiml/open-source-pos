using FluentValidation;

namespace LiquorPOS.Services.Identity.Application.Commands.AssignUserRoles;

public sealed class AssignUserRolesCommandValidator : AbstractValidator<AssignUserRolesCommand>
{
    public AssignUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Roles)
            .NotNull()
            .WithMessage("Roles collection cannot be null")
            .NotEmpty()
            .WithMessage("At least one role must be specified");

        RuleForEach(x => x.Roles)
            .NotEmpty()
            .WithMessage("Role name cannot be empty")
            .MaximumLength(100)
            .WithMessage("Role name cannot exceed 100 characters");
    }
}