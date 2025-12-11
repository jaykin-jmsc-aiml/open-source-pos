using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Commands.RefreshToken;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Infrastructure.Services;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.Services;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Security;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<LiquorPOSIdentityDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            _dbContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand("");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token is required");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand("non_existent_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .Returns(DbSetMockHelper.CreateMockDbSetFromList(new List<RefreshToken>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid refresh token");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithRevokedToken_ShouldReturnFailure()
    {
        // Arrange
        var revokedToken = RefreshToken.CreateWithHash(
            Guid.NewGuid(),
            TokenHasher.Hash("revoked_token"),
            DateTime.UtcNow.AddDays(7));
        revokedToken.Revoke();

        var command = new RefreshTokenCommand("revoked_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .Returns(DbSetMockHelper.CreateMockDbSetFromList(new[] { revokedToken }));

        _jwtTokenServiceMock.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token has been revoked. Please login again.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var expiredToken = RefreshToken.CreateWithHash(
            Guid.NewGuid(),
            TokenHasher.Hash("expired_token"),
            DateTime.UtcNow.AddMinutes(-10));

        var command = new RefreshTokenCommand("expired_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .Returns(DbSetMockHelper.CreateMockDbSetFromList(new[] { expiredToken }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token has expired. Please login again.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var validToken = RefreshToken.CreateWithHash(
            Guid.NewGuid(),
            TokenHasher.Hash("valid_token"),
            DateTime.UtcNow.AddDays(7));

        var command = new RefreshTokenCommand("valid_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .Returns(DbSetMockHelper.CreateMockDbSetFromList(new[] { validToken }));

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var validToken = RefreshToken.CreateWithHash(
            userId,
            TokenHasher.Hash("valid_token"),
            DateTime.UtcNow.AddDays(7));

        var inactiveUser = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = false
        };

        var command = new RefreshTokenCommand("valid_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .Returns(DbSetMockHelper.CreateMockDbSetFromList(new[] { validToken }));

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(inactiveUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User account is inactive");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithValidTokenAndActiveUser_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var validToken = RefreshToken.CreateWithHash(
            userId,
            TokenHasher.Hash("valid_token"),
            DateTime.UtcNow.AddDays(7));

        var activeUser = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };

        var expectedAuthResponse = new AuthResponse(
            "new_access_token",
            "new_refresh_token",
            15,
            activeUser.Id,
            activeUser.Email!,
            activeUser.FirstName,
            activeUser.LastName);

        var command = new RefreshTokenCommand("valid_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .Returns(DbSetMockHelper.CreateMockDbSetFromList(new[] { validToken }));

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(activeUser);

        _jwtTokenServiceMock.Setup(x => x.RefreshTokensAsync("valid_token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(("new_access_token", "new_refresh_token"));

        var auditLogsDbSet = new Mock<DbSet<AuditLog>>();
        auditLogsDbSet.Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<EntityEntry<AuditLog>>(Task.FromResult(default(EntityEntry<AuditLog>)!)));

        _dbContextMock.Setup(x => x.AuditLogs).Returns(auditLogsDbSet.Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Token refreshed successfully");
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(expectedAuthResponse);
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .Throws(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during token refresh");
        result.Data.Should().BeNull();
    }
}