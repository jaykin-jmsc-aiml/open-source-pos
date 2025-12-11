using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Commands.RevokeToken;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Infrastructure.Services;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.Services;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Security;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Commands;

public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<LiquorPOSIdentityDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<RevokeTokenCommandHandler>> _loggerMock = new();
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new RevokeTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            _dbContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new RevokeTokenCommand("");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token is required");
    }

    [Fact]
    public async Task Handle_WithNonExistentToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new RevokeTokenCommand("non_existent_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .ReturnsDbSet(new List<RefreshToken>().AsQueryable());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_ShouldReturnSuccess()
    {
        // Arrange
        var alreadyRevokedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = TokenHasher.Hash("already_revoked_token"),
            RevokedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var command = new RevokeTokenCommand("already_revoked_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .ReturnsDbSet(new[] { alreadyRevokedToken }.AsQueryable());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Token already revoked");
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var validToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = TokenHasher.Hash("valid_token"),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var command = new RevokeTokenCommand("valid_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .ReturnsDbSet(new[] { validToken }.AsQueryable());

        _jwtTokenServiceMock.Setup(x => x.RevokeRefreshTokenAsync("valid_token", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dbContextMock.Setup(x => x.AuditLogs.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Token revoked successfully");

        _jwtTokenServiceMock.Verify(x => x.RevokeRefreshTokenAsync("valid_token", It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(x => x.AuditLogs.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        var command = new RevokeTokenCommand("valid_token");
        
        _dbContextMock.Setup(x => x.RefreshTokens)
            .Throws(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during token revocation");
    }
}