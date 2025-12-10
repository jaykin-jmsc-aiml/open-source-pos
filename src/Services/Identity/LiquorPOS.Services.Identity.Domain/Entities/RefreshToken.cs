using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.Identity.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    private RefreshToken() { }

    private RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public static RefreshToken Create(Guid userId, TimeSpan? duration = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var token = GenerateToken();
        var expiresAt = DateTime.UtcNow.Add(duration ?? TimeSpan.FromDays(7));

        return new RefreshToken(userId, token, expiresAt);
    }

    public static RefreshToken CreateOrThrow(Guid userId, TimeSpan? duration = null)
    {
        return Create(userId, duration);
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsValid => !IsExpired && !IsRevoked;

    public RefreshToken Rotate(TimeSpan? duration = null)
    {
        if (!IsValid)
            throw new InvalidOperationException("Cannot rotate an expired or revoked token");

        var newToken = Create(UserId, duration);
        ReplacedByToken = newToken.Token;
        RevokedAt = DateTime.UtcNow;

        return newToken;
    }

    public void Revoke()
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
    }

    private static string GenerateToken()
    {
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            var buffer = new byte[32];
            rng.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }
    }
}
