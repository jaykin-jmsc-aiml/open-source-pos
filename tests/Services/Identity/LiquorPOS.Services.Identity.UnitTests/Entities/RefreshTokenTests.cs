using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.Entities;

namespace LiquorPOS.Services.Identity.UnitTests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidUserId_ShouldSucceed()
    {
        var userId = Guid.NewGuid();

        var (token, plainToken) = RefreshToken.Create(userId);

        token.Should().NotBeNull();
        token.UserId.Should().Be(userId);
        plainToken.Should().NotBeEmpty();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        token.RevokedAt.Should().BeNull();
        token.ReplacedByTokenHash.Should().BeNull();
        token.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithCustomDuration_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var duration = TimeSpan.FromDays(14);

        var (token, _) = RefreshToken.Create(userId, duration);

        token.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(duration), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowArgumentException()
    {
        var action = () => RefreshToken.Create(Guid.Empty);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsExpired_WithFutureExpiration_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        var (token, _) = RefreshToken.Create(userId);

        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastExpiration_ShouldReturnTrue()
    {
        var userId = Guid.NewGuid();
        var (token, _) = RefreshToken.Create(userId, TimeSpan.FromMilliseconds(1));

        System.Threading.Thread.Sleep(100);

        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsRevoked_WithoutRevocation_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        var (token, _) = RefreshToken.Create(userId);

        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void IsRevoked_AfterRevocation_ShouldReturnTrue()
    {
        var userId = Guid.NewGuid();
        var (token, _) = RefreshToken.Create(userId);
        token.Revoke();

        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithValidToken_ShouldReturnTrue()
    {
        var userId = Guid.NewGuid();
        var (token, _) = RefreshToken.Create(userId);

        token.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithRevokedToken_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        var (token, _) = RefreshToken.Create(userId);
        token.Revoke();

        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithExpiredToken_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        var (token, _) = RefreshToken.Create(userId, TimeSpan.FromMilliseconds(1));

        System.Threading.Thread.Sleep(100);

        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Revoke_WithValidToken_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var (token, _) = RefreshToken.Create(userId);

        token.Revoke();

        token.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_WithAlreadyRevokedToken_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var (token, _) = RefreshToken.Create(userId);
        token.Revoke();

        var action = () => token.Revoke();

        action.Should().NotThrow();
        token.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueTokens()
    {
        var userId = Guid.NewGuid();
        var (_, plainToken1) = RefreshToken.Create(userId);
        var (_, plainToken2) = RefreshToken.Create(userId);

        plainToken1.Should().NotBe(plainToken2);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var userId = Guid.NewGuid();
        var (token1, _) = RefreshToken.Create(userId);
        var (token2, _) = RefreshToken.Create(userId);

        token1.Id.Should().NotBe(token2.Id);
    }

    [Fact]
    public void MarkAsRotated_ShouldSetReplacedByTokenHash()
    {
        var userId = Guid.NewGuid();
        var (oldToken, _) = RefreshToken.Create(userId);
        var newTokenHash = "new-token-hash";

        oldToken.MarkAsRotated(newTokenHash);

        oldToken.IsRevoked.Should().BeTrue();
        oldToken.ReplacedByTokenHash.Should().Be(newTokenHash);
        oldToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void CreateWithHash_ShouldCreateTokenWithHash()
    {
        var userId = Guid.NewGuid();
        var tokenHash = "test-token-hash";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var token = RefreshToken.CreateWithHash(userId, tokenHash, expiresAt);

        token.Should().NotBeNull();
        token.UserId.Should().Be(userId);
        token.TokenHash.Should().Be(tokenHash);
        token.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void GenerateToken_ShouldReturnNonEmptyString()
    {
        var token = RefreshToken.GenerateToken();

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_ShouldReturnUniqueTokens()
    {
        var token1 = RefreshToken.GenerateToken();
        var token2 = RefreshToken.GenerateToken();

        token1.Should().NotBe(token2);
    }
}
