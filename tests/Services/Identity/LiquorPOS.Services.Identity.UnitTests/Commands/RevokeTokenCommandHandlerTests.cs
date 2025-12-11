using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Commands.RevokeToken;
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

public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<RevokeTokenCommandHandler>> _loggerMock = new();

    public RevokeTokenCommandHandlerTests()
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
        var handler = new RevokeTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);
        var command = new RevokeTokenCommand("");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token is required");
    }

    [Fact]
    public async Task Handle_WithNonExistentToken_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RevokeTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);
        var command = new RevokeTokenCommand("non_existent_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_ShouldReturnSuccess()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RevokeTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);

        var userId = Guid.NewGuid();
        var alreadyRevokedToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("already_revoked_token")
            .WithRevoked()
            .Build();

        dbContext.RefreshTokens.Add(alreadyRevokedToken);
        await dbContext.SaveChangesAsync();

        var command = new RevokeTokenCommand("already_revoked_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Token already revoked");
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RevokeTokenCommandHandler(
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

        _jwtTokenServiceMock.Setup(x => x.RevokeRefreshTokenAsync("valid_token", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RevokeTokenCommand("valid_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Token revoked successfully");

        _jwtTokenServiceMock.Verify(x => x.RevokeRefreshTokenAsync("valid_token", It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify the token is actually revoked in the database
        var revokedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == TokenHasher.Hash("valid_token"), cancellationToken: CancellationToken.None);
        revokedToken.Should().NotBeNull();
        revokedToken!.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var handler = new RevokeTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            dbContext,
            _loggerMock.Object);

        var command = new RevokeTokenCommand("valid_token");

        // Act & Assert - Simulate database error by using a disposed context
        await using var disposedContext = new LiquorPOSIdentityDbContext(
            new DbContextOptionsBuilder<LiquorPOSIdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        
        disposedContext.Dispose();

        var handlerWithError = new RevokeTokenCommandHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            disposedContext,
            _loggerMock.Object);

        var result = await handlerWithError.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during token revocation");
    }
}