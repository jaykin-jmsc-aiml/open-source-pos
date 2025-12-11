using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Commands.AssignUserRoles;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Commands;

public class AssignUserRolesCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<LiquorPOSIdentityDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<AssignUserRolesCommandHandler>> _loggerMock = new();
    private readonly AssignUserRolesCommandHandler _handler;

    public AssignUserRolesCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            Mock.Of<IRoleStore<ApplicationRole>>(),
            null!, null!, null!, null!);

        _handler = new AssignUserRolesCommandHandler(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _dbContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var command = new AssignUserRolesCommand(Guid.Empty, new List<string> { "Admin" }.AsReadOnly());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User ID is required");
    }

    [Fact]
    public async Task Handle_WithNullRoles_ShouldReturnFailure()
    {
        // Arrange
        var command = new AssignUserRolesCommand(Guid.NewGuid(), null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("At least one role must be specified");
    }

    [Fact]
    public async Task Handle_WithEmptyRoles_ShouldReturnFailure()
    {
        // Arrange
        var command = new AssignUserRolesCommand(Guid.NewGuid(), new List<string>().AsReadOnly());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("At least one role must be specified");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new AssignUserRolesCommand(userId, new List<string> { "Admin" }.AsReadOnly());

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_WithInvalidRoles_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var command = new AssignUserRolesCommand(userId, new List<string> { "InvalidRole" }.AsReadOnly());

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("InvalidRole"))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid roles");
    }

    [Fact]
    public async Task Handle_WithValidData_AddingNewRoles_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var currentRoles = new List<string> { "User" };
        var newRoles = new List<string> { "User", "Admin" };
        var command = new AssignUserRolesCommand(userId, newRoles.AsReadOnly());

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("User"))
            .ReturnsAsync(true);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("Admin"))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Admin"))))
            .ReturnsAsync(IdentityResult.Success);

        _dbContextMock.Setup(x => x.AuditLogs.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<EntityEntry<AuditLog>>(new Mock<EntityEntry<AuditLog>>().Object));

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Roles assigned successfully");

        _userManagerMock.Verify(x => x.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Admin"))), Times.Once);
        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidData_RemovingRoles_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var currentRoles = new List<string> { "Admin", "Manager", "User" };
        var newRoles = new List<string> { "User" };
        var command = new AssignUserRolesCommand(userId, newRoles.AsReadOnly());

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("User"))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Admin") && r.Contains("Manager"))))
            .ReturnsAsync(IdentityResult.Success);

        _dbContextMock.Setup(x => x.AuditLogs.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<EntityEntry<AuditLog>>(new Mock<EntityEntry<AuditLog>>().Object));

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Roles assigned successfully");

        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Admin") && r.Contains("Manager"))), Times.Once);
        _userManagerMock.Verify(x => x.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAddRolesFailure_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var currentRoles = new List<string> { "User" };
        var newRoles = new List<string> { "User", "Admin" };
        var command = new AssignUserRolesCommand(userId, newRoles.AsReadOnly());

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("User"))
            .ReturnsAsync(true);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("Admin"))
            .ReturnsAsync(true);

        var errorResult = IdentityResult.Failed(new IdentityError { Description = "Failed to add role" });
        _userManagerMock.Setup(x => x.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(errorResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to assign roles");
    }

    [Fact]
    public async Task Handle_WithRemoveRolesFailure_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var currentRoles = new List<string> { "Admin", "User" };
        var newRoles = new List<string> { "User" };
        var command = new AssignUserRolesCommand(userId, newRoles.AsReadOnly());

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("User"))
            .ReturnsAsync(true);

        var errorResult = IdentityResult.Failed(new IdentityError { Description = "Failed to remove role" });
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(errorResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to remove roles");
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        var command = new AssignUserRolesCommand(Guid.NewGuid(), new List<string> { "Admin" }.AsReadOnly());

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .Throws(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while assigning roles");
    }
}