using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.Identity.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    private RefreshToken() { }

    private RefreshToken(Guid userId, string tokenHash, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public static (RefreshToken Entity, string PlainToken) Create(Guid userId, TimeSpan? duration = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var plainToken = GenerateToken();
        var expiresAt = DateTime.UtcNow.Add(duration ?? TimeSpan.FromDays(7));

        var entity = new RefreshToken(userId, string.Empty, expiresAt);
        return (entity, plainToken);
    }

    public static RefreshToken CreateWithHash(Guid userId, string tokenHash, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash cannot be empty", nameof(tokenHash));

        return new RefreshToken(userId, tokenHash, expiresAt);
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsValid => !IsExpired && !IsRevoked;

    public void MarkAsRotated(string newTokenHash)
    {
        if (string.IsNullOrWhiteSpace(newTokenHash))
            throw new ArgumentException("New token hash cannot be empty", nameof(newTokenHash));

        ReplacedByTokenHash = newTokenHash;
        RevokedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
    }

    public static string GenerateToken()
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var buffer = new byte[32];
        rng.GetBytes(buffer);
        return Convert.ToBase64String(buffer);
    }
}
