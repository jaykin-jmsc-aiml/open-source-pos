using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Security;

namespace LiquorPOS.Services.Identity.UnitTests.TestHelpers.Builders;

public class RefreshTokenBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private string _token = RefreshToken.GenerateToken();
    private DateTime _expiresAt = DateTime.UtcNow.AddDays(7);
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _revokedAt = null;
    private string? _replacedByTokenHash = null;
    private bool _createWithHash = false;

    public RefreshTokenBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RefreshTokenBuilder ForUser(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public RefreshTokenBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public RefreshTokenBuilder WithExpiration(DateTime expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    public RefreshTokenBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public RefreshTokenBuilder WithRevoked()
    {
        _revokedAt = DateTime.UtcNow;
        return this;
    }

    public RefreshTokenBuilder WithRevokedAt(DateTime revokedAt)
    {
        _revokedAt = revokedAt;
        return this;
    }

    public RefreshTokenBuilder ReplacedBy(string tokenHash)
    {
        _replacedByTokenHash = tokenHash;
        _revokedAt = DateTime.UtcNow;
        return this;
    }

    public RefreshTokenBuilder CreateWithHash(bool createWithHash = true)
    {
        _createWithHash = createWithHash;
        return this;
    }

    public RefreshToken Build()
    {
        // Always hash the token for consistency
        var tokenHash = TokenHasher.Hash(_token);
        var refreshToken = RefreshToken.CreateWithHash(_userId, tokenHash, _expiresAt);
        refreshToken.Id = _id;
        refreshToken.CreatedAt = _createdAt;

        if (_revokedAt.HasValue)
        {
            refreshToken.Revoke();
        }

        if (!string.IsNullOrEmpty(_replacedByTokenHash))
        {
            refreshToken.MarkAsRotated(_replacedByTokenHash);
        }

        return refreshToken;
    }
}