using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Application.Queries;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<LiquorPOSIdentityDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<GetUserByIdQueryHandler>> _loggerMock = new();
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new GetUserByIdQueryHandler(
            _userManagerMock.Object,
            _dbContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.Empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User ID is required");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1234567890",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            LastModifiedAt = DateTime.UtcNow.AddHours(-2),
            LastLoginAt = DateTime.UtcNow.AddMinutes(-30)
        };

        var expectedRoles = new List<string> { "Admin", "User" };

        var query = new GetUserByIdQuery(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("User retrieved successfully");
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(userId);
        result.Data.Email.Should().Be("test@example.com");
        result.Data.FirstName.Should().Be("Test");
        result.Data.LastName.Should().Be("User");
        result.Data.PhoneNumber.Should().Be("+1234567890");
        result.Data.IsActive.Should().BeTrue();
        result.Data.Roles.Should().BeEquivalentTo(expectedRoles);
    }

    [Fact]
    public async Task Handle_WithUserWithoutRoles_ShouldReturnEmptyRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var expectedRoles = new List<string>();

        var query = new GetUserByIdQuery(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .Throws(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving user");
        result.Data.Should().BeNull();
    }
}