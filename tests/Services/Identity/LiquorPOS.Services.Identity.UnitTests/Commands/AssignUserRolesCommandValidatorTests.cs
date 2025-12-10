using FluentAssertions;
using FluentValidation.TestHelper;
using LiquorPOS.Services.Identity.Application.Commands.AssignUserRoles;

namespace LiquorPOS.Services.Identity.UnitTests.Commands;

public class AssignUserRolesCommandValidatorTests
{
    private readonly AssignUserRolesCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new AssignUserRolesCommand(
            Guid.NewGuid(), 
            new List<string> { "Admin", "User" }.AsReadOnly());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldFail()
    {
        var command = new AssignUserRolesCommand(
            Guid.Empty, 
            new List<string> { "Admin", "User" }.AsReadOnly());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required");
    }

    [Fact]
    public void Validate_WithNullRoles_ShouldFail()
    {
        var command = new AssignUserRolesCommand(
            Guid.NewGuid(), 
            null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Roles)
            .WithErrorMessage("Roles collection cannot be null");
    }

    [Fact]
    public void Validate_WithEmptyRoles_ShouldFail()
    {
        var command = new AssignUserRolesCommand(
            Guid.NewGuid(), 
            new List<string>().AsReadOnly());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Roles)
            .WithErrorMessage("At least one role must be specified");
    }

    [Fact]
    public void Validate_WithEmptyRoleName_ShouldFail()
    {
        var command = new AssignUserRolesCommand(
            Guid.NewGuid(), 
            new List<string> { "Admin", "", "User" }.AsReadOnly());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Roles[1]")
            .WithErrorMessage("Role name cannot be empty");
    }

    [Fact]
    public void Validate_WithLongRoleName_ShouldFail()
    {
        var longRoleName = new string('a', 101);
        var command = new AssignUserRolesCommand(
            Guid.NewGuid(), 
            new List<string> { "Admin", longRoleName, "User" }.AsReadOnly());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Roles[1]")
            .WithErrorMessage("Role name cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_WithSingleValidRole_ShouldSucceed()
    {
        var command = new AssignUserRolesCommand(
            Guid.NewGuid(), 
            new List<string> { "Admin" }.AsReadOnly());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}