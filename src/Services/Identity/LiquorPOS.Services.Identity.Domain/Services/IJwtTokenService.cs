using LiquorPOS.Services.Identity.Domain.Entities;

namespace LiquorPOS.Services.Identity.Domain.Services;

public interface IJwtTokenService
{
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user, CancellationToken cancellationToken = default);
    Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}