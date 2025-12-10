using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.Entities;

namespace LiquorPOS.Services.Identity.UnitTests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidUserId_ShouldSucceed()
    {
        var userId = Guid.NewGuid();

        var token = RefreshToken.Create(userId);

        token.Should().NotBeNull();
        token.UserId.Should().Be(userId);
        token.Token.Should().NotBeEmpty();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        token.RevokedAt.Should().BeNull();
        token.ReplacedByToken.Should().BeNull();
        token.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithCustomDuration_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var duration = TimeSpan.FromDays(14);

        var token = RefreshToken.Create(userId, duration);

        token.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(duration), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowArgumentException()
    {
        var action = () => RefreshToken.Create(Guid.Empty);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateOrThrow_WithValidUserId_ShouldReturnToken()
    {
        var userId = Guid.NewGuid();

        var token = RefreshToken.CreateOrThrow(userId);

        token.Should().NotBeNull();
        token.UserId.Should().Be(userId);
    }

    [Fact]
    public void CreateOrThrow_WithEmptyUserId_ShouldThrowArgumentException()
    {
        var action = () => RefreshToken.CreateOrThrow(Guid.Empty);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsExpired_WithFutureExpiration_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId);

        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastExpiration_ShouldReturnTrue()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, TimeSpan.FromMilliseconds(1));

        System.Threading.Thread.Sleep(100);

        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsRevoked_WithoutRevocation_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId);

        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void IsRevoked_AfterRevocation_ShouldReturnTrue()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId);
        token.Revoke();

        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithValidToken_ShouldReturnTrue()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId);

        token.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithRevokedToken_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId);
        token.Revoke();

        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithExpiredToken_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, TimeSpan.FromMilliseconds(1));

        System.Threading.Thread.Sleep(100);

        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Rotate_WithValidToken_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var oldToken = RefreshToken.Create(userId);
        var oldTokenValue = oldToken.Token;

        var newToken = oldToken.Rotate();

        newToken.Should().NotBeNull();
        newToken.UserId.Should().Be(userId);
        newToken.Token.Should().NotBe(oldTokenValue);
        newToken.IsValid.Should().BeTrue();

        oldToken.IsRevoked.Should().BeTrue();
        oldToken.ReplacedByToken.Should().Be(newToken.Token);
        oldToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void Rotate_WithRevokedToken_ShouldThrowInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId);
        token.Revoke();

        var action = () => token.Rotate();

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Rotate_WithExpiredToken_ShouldThrowInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, TimeSpan.FromMilliseconds(1));

        System.Threading.Thread.Sleep(100);

        var action = () => token.Rotate();

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Rotate_WithCustomDuration_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var oldToken = RefreshToken.Create(userId);
        var duration = TimeSpan.FromDays(14);

        var newToken = oldToken.Rotate(duration);

        newToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(duration), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_WithValidToken_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId);

        token.Revoke();

        token.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_WithAlreadyRevokedToken_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId);
        token.Revoke();

        var action = () => token.Revoke();

        action.Should().NotThrow();
        token.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueTokens()
    {
        var userId = Guid.NewGuid();
        var token1 = RefreshToken.Create(userId);
        var token2 = RefreshToken.Create(userId);

        token1.Token.Should().NotBe(token2.Token);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var userId = Guid.NewGuid();
        var token1 = RefreshToken.Create(userId);
        var token2 = RefreshToken.Create(userId);

        token1.Id.Should().NotBe(token2.Id);
    }
}
