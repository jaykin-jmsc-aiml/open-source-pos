using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.Options;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.ValueObjects;
using LiquorPOS.Services.Identity.Domain.Services;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Services;

public sealed class JwtTokenServiceTests : IDisposable
{
    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly JwtOptions _jwtOptions;
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var options = new DbContextOptionsBuilder<LiquorPOSIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LiquorPOSIdentityDbContext(options);

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _jwtOptions = new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "ThisIsATestSigningKeyWith32PlusCharacters!",
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 7
        };

        var optionsMock = new Mock<IOptions<JwtOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_jwtOptions);

        _sut = new JwtTokenService(_dbContext, _userManagerMock.Object, optionsMock.Object);
    }

    [Fact]
    public async Task GenerateTokensAsync_ShouldReturnAccessTokenAndRefreshToken()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string> { "Admin" });

        var (accessToken, refreshToken) = await _sut.GenerateTokensAsync(user);

        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateTokensAsync_AccessToken_ShouldContainExpectedClaims()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string> { "Admin", "Manager" });

        var (accessToken, _) = await _sut.GenerateTokensAsync(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email.Value);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.GivenName && c.Value == user.FirstName);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.FamilyName && c.Value == user.LastName);
        jwtToken.Claims.Should().Contain(c => c.Type == "userId" && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Manager");
    }

    [Fact]
    public async Task GenerateTokensAsync_AccessToken_ShouldExpireIn15Minutes()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string>());

        var (accessToken, _) = await _sut.GenerateTokensAsync(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GenerateTokensAsync_RefreshToken_ShouldBeStoredInDatabase()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string>());

        var (_, refreshToken) = await _sut.GenerateTokensAsync(user);

        var storedTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync();

        storedTokens.Should().HaveCount(1);
        storedTokens[0].UserId.Should().Be(user.Id);
        storedTokens[0].IsExpired.Should().BeFalse();
        storedTokens[0].IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateTokensAsync_RefreshToken_ShouldExpireIn7Days()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string>());

        await _sut.GenerateTokensAsync(user);

        var storedToken = await _dbContext.RefreshTokens
            .FirstAsync(rt => rt.UserId == user.Id);

        var expectedExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetimeDays);
        storedToken.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GenerateTokensAsync_ShouldThrowException_WhenUserIsInactive()
    {
        var user = CreateTestUser();
        user.Deactivate();

        var act = async () => await _sut.GenerateTokensAsync(user);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot generate tokens for inactive user");
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldGenerateNewTokenPair()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string> { "Admin" });

        var (_, oldRefreshToken) = await _sut.GenerateTokensAsync(user);

        var (newAccessToken, newRefreshToken) = await _sut.RefreshTokensAsync(oldRefreshToken);

        newAccessToken.Should().NotBeNullOrEmpty();
        newRefreshToken.Should().NotBeNullOrEmpty();
        newRefreshToken.Should().NotBe(oldRefreshToken);
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldRevokeOldToken()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string>());

        var (_, oldRefreshToken) = await _sut.GenerateTokensAsync(user);

        await _sut.RefreshTokensAsync(oldRefreshToken);

        var allTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .OrderBy(rt => rt.CreatedAt)
            .ToListAsync();

        allTokens.Should().HaveCount(2);
        allTokens[0].IsRevoked.Should().BeTrue();
        allTokens[0].RevokedAt.Should().NotBeNull();
        allTokens[1].IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldThrowException_WhenTokenIsInvalid()
    {
        var invalidToken = "invalid-token";

        var act = async () => await _sut.RefreshTokensAsync(invalidToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid refresh token");
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldThrowException_WhenTokenIsExpired()
    {
        var user = CreateTestUser();
        var plainToken = RefreshToken.GenerateToken();
        var tokenHash = Infrastructure.Security.TokenHasher.Hash(plainToken);

        var expiredToken = RefreshToken.CreateWithHash(
            user.Id,
            tokenHash,
            DateTime.UtcNow.AddDays(-1));

        await _dbContext.RefreshTokens.AddAsync(expiredToken);
        await _dbContext.SaveChangesAsync();

        var act = async () => await _sut.RefreshTokensAsync(plainToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Refresh token has expired");
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldRevokeAllTokens_WhenRevokedTokenIsReused()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string>());

        var (_, oldRefreshToken) = await _sut.GenerateTokensAsync(user);

        await _sut.RefreshTokensAsync(oldRefreshToken);

        var act = async () => await _sut.RefreshTokensAsync(oldRefreshToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Refresh token has been revoked. Possible token reuse detected.");

        var allTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync();

        allTokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldRevokeToken()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string>());

        var (_, refreshToken) = await _sut.GenerateTokensAsync(user);

        await _sut.RevokeRefreshTokenAsync(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstAsync(rt => rt.UserId == user.Id);

        storedToken.IsRevoked.Should().BeTrue();
        storedToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeAllUserRefreshTokensAsync_ShouldRevokeAllUserTokens()
    {
        var user = CreateTestUser();
        var applicationUser = CreateTestApplicationUser(user);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(applicationUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string>());

        await _sut.GenerateTokensAsync(user);
        await _sut.GenerateTokensAsync(user);
        await _sut.GenerateTokensAsync(user);

        await _sut.RevokeAllUserRefreshTokensAsync(user.Id);

        var allTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync();

        allTokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }

    private User CreateTestUser()
    {
        var email = Email.Create($"test{Guid.NewGuid()}@example.com");
        var password = PasswordHash.Create("TestPassword123!");

        return User.Create(
            email.Value!,
            "John",
            "Doe",
            password.Value!);
    }

    private ApplicationUser CreateTestApplicationUser(User user)
    {
        return new ApplicationUser
        {
            Id = user.Id,
            Email = user.Email.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            UserName = user.Email.Value,
            NormalizedUserName = user.Email.Value.ToUpperInvariant()
        };
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
