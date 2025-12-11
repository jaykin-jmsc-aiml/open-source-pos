using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Commands.RefreshToken;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Infrastructure.Services;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.Services;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Security;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock = new();

    public RefreshTokenCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Handle_WithEmptyRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);
        var command = new RefreshTokenCommand("");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token is required");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentToken_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);
        var command = new RefreshTokenCommand("non_existent_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid refresh token");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithRevokedToken_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);

        var userId = Guid.NewGuid();
        var revokedToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("revoked_token")
            .WithRevoked()
            .Build();

        dbContext.RefreshTokens.Add(revokedToken);
        await dbContext.SaveChangesAsync();

        _jwtTokenServiceMock.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RefreshTokenCommand("revoked_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token has been revoked. Please login again.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);

        var userId = Guid.NewGuid();
        var expiredToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("expired_token")
            .WithExpiration(DateTime.UtcNow.AddMinutes(-10))
            .Build();

        dbContext.RefreshTokens.Add(expiredToken);
        await dbContext.SaveChangesAsync();

        var command = new RefreshTokenCommand("expired_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token has expired. Please login again.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);

        var userId = Guid.NewGuid();
        var validToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("valid_token")
            .Build();

        dbContext.RefreshTokens.Add(validToken);
        await dbContext.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var command = new RefreshTokenCommand("valid_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);

        var userId = Guid.NewGuid();
        var validToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("valid_token")
            .Build();

        dbContext.RefreshTokens.Add(validToken);
        await dbContext.SaveChangesAsync();

        var inactiveUser = new UserBuilder()
            .WithId(userId)
            .WithEmail("test@example.com")
            .WithIsActive(false)
            .Build();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(inactiveUser);

        var command = new RefreshTokenCommand("valid_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User account is inactive");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithValidTokenAndActiveUser_ShouldReturnSuccess()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);

        var userId = Guid.NewGuid();
        var validToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("valid_token")
            .Build();

        dbContext.RefreshTokens.Add(validToken);
        await dbContext.SaveChangesAsync();

        var activeUser = new UserBuilder()
            .WithId(userId)
            .WithEmail("test@example.com")
            .WithIsActive(true)
            .Build();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(activeUser);

        _jwtTokenServiceMock.Setup(x => x.RefreshTokensAsync("valid_token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(("new_access_token", "new_refresh_token"));

        var command = new RefreshTokenCommand("valid_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Token refreshed successfully");
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().Be("new_access_token");
        result.Data.RefreshToken.Should().Be("new_refresh_token");
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);

        var command = new RefreshTokenCommand("valid_token");

        // Act & Assert - Simulate database error by trying to access a disposed context
        await using var disposedContext = new LiquorPOSIdentityDbContext(
            new DbContextOptionsBuilder<LiquorPOSIdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        
        disposedContext.Dispose();

        var handlerWithError = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            disposedContext,
            _loggerMock.Object);

        var result = await handlerWithError.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during token refresh");
        result.Data.Should().BeNull();
    }
}