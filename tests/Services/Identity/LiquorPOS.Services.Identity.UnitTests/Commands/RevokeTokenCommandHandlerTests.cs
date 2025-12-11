using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Commands.RevokeToken;
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

public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<ILogger<RevokeTokenCommandHandler>> _loggerMock = new();

    private (IJwtTokenService, UserManager<ApplicationUser>, LiquorPOSIdentityDbContext, RevokeTokenCommandHandler) CreateHandler()
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
        var handler = new RevokeTokenCommandHandler(jwtTokenService, userManager, dbContext, _loggerMock.Object);
        
        return (jwtTokenService, userManager, dbContext, handler);
    }

    [Fact]
    public async Task Handle_WithEmptyRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();
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
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();
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
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();

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
        var (jwtTokenService, userManager, dbContext, handler) = CreateHandler();

        var userId = Guid.NewGuid();
        var validToken = new RefreshTokenBuilder()
            .ForUser(userId)
            .WithToken("valid_token")
            .Build();

        dbContext.RefreshTokens.Add(validToken);
        await dbContext.SaveChangesAsync();

        var command = new RevokeTokenCommand("valid_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Token revoked successfully");
        
        // Verify the token is actually revoked in the database
        var revokedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == TokenHasher.Hash("valid_token"), cancellationToken: CancellationToken.None);
        revokedToken.Should().NotBeNull();
        revokedToken!.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        var command = new RevokeTokenCommand("valid_token");

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
        var handlerWithError = new RevokeTokenCommandHandler(jwtTokenService, userManager, disposedContext, _loggerMock.Object);
        
        disposedContext.Dispose();

        var result = await handlerWithError.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during token revocation");
    }
}