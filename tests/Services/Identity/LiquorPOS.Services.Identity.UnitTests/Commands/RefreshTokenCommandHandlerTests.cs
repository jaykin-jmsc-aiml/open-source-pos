using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Commands.RefreshToken;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Infrastructure.Services;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.Options;
using LiquorPOS.Services.Identity.Domain.Services;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Security;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock = new();

    private (IJwtTokenService, UserManager<ApplicationUser>, LiquorPOSIdentityDbContext, RefreshTokenCommandHandler) CreateHandler()
    {
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var userStore = new UserStore<ApplicationUser, IdentityRole<Guid>, LiquorPOSIdentityDbContext, Guid>(dbContext);
        var userManager = new UserManager<ApplicationUser>(
            userStore,
            null,
            new PasswordHasher<ApplicationUser>(),
            null,
            null,
            null,
            null,
            null,
            null);
        
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "ThisIsATestSigningKeyWith32PlusCharacters!",
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 7
        });
        
        var jwtTokenService = new JwtTokenService(dbContext, userManager, jwtOptions);
        var handler = new RefreshTokenCommandHandler(jwtTokenService, userManager, dbContext, _loggerMock.Object);
        
        return (jwtTokenService, userManager, dbContext, handler);
    }

    [Fact]
    public async Task Handle_WithEmptyRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();
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
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();
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
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();

        var userId = Guid.NewGuid();
        var revokedToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("revoked_token")
            .WithRevoked()
            .Build();

        dbContext.RefreshTokens.Add(revokedToken);
        await dbContext.SaveChangesAsync();

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
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();

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
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();

        var userId = Guid.NewGuid();
        var validToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("valid_token")
            .Build();

        dbContext.RefreshTokens.Add(validToken);
        await dbContext.SaveChangesAsync();

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
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();

        var userId = Guid.NewGuid();
        var inactiveUser = new UserBuilder()
            .WithId(userId)
            .WithEmail("test@example.com")
            .WithIsActive(false)
            .Build();

        await userManager.CreateAsync(inactiveUser);

        var validToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("valid_token")
            .Build();

        dbContext.RefreshTokens.Add(validToken);
        await dbContext.SaveChangesAsync();

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
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();

        var userId = Guid.NewGuid();
        var activeUser = new UserBuilder()
            .WithId(userId)
            .WithEmail("test@example.com")
            .WithIsActive(true)
            .Build();

        await userManager.CreateAsync(activeUser);

        var validToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("valid_token")
            .Build();

        dbContext.RefreshTokens.Add(validToken);
        await dbContext.SaveChangesAsync();

        var command = new RefreshTokenCommand("valid_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue($"because token refresh should succeed, but got message: {result.Message}");
        result.Message.Should().Be("Token refreshed successfully");
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid_token");

        // Act & Assert - Simulate database error by using a disposed context
        var disposedContext = new LiquorPOSIdentityDbContext(
            new DbContextOptionsBuilder<LiquorPOSIdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        
        var userStore = new UserStore<ApplicationUser, IdentityRole<Guid>, LiquorPOSIdentityDbContext, Guid>(disposedContext);
        var userManager = new UserManager<ApplicationUser>(
            userStore,
            null,
            new PasswordHasher<ApplicationUser>(),
            null,
            null,
            null,
            null,
            null,
            null);
        
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "ThisIsATestSigningKeyWith32PlusCharacters!",
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 7
        });
        
        var jwtTokenService = new JwtTokenService(disposedContext, userManager, jwtOptions);
        var handlerWithError = new RefreshTokenCommandHandler(jwtTokenService, userManager, disposedContext, _loggerMock.Object);
        
        disposedContext.Dispose();

        var result = await handlerWithError.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during token refresh");
        result.Data.Should().BeNull();
    }
}