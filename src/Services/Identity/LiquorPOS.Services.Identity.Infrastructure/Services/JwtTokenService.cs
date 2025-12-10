using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.Options;
using LiquorPOS.Services.Identity.Domain.Services;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LiquorPOS.Services.Identity.Infrastructure.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtOptions _jwtOptions;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtTokenService(
        LiquorPOSIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _tokenHandler = new JwtSecurityTokenHandler();

        ValidateOptions();
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Issuer))
            throw new InvalidOperationException("JWT Issuer is not configured");

        if (string.IsNullOrWhiteSpace(_jwtOptions.Audience))
            throw new InvalidOperationException("JWT Audience is not configured");

        if (string.IsNullOrWhiteSpace(_jwtOptions.SigningKey))
            throw new InvalidOperationException("JWT SigningKey is not configured");

        if (_jwtOptions.SigningKey.Length < 32)
            throw new InvalidOperationException("JWT SigningKey must be at least 32 characters long");
    }

    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (!user.IsActive)
            throw new InvalidOperationException("Cannot generate tokens for inactive user");

        var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (applicationUser == null)
            throw new InvalidOperationException("Application user not found");

        var accessToken = await GenerateAccessTokenAsync(applicationUser, cancellationToken);
        var (refreshTokenEntity, plainRefreshToken) = await CreateRefreshTokenAsync(user.Id, cancellationToken);

        return (accessToken, plainRefreshToken);
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be empty", nameof(refreshToken));

        var tokenHash = TokenHasher.Hash(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken == null)
            throw new InvalidOperationException("Invalid refresh token");

        if (storedToken.IsRevoked)
        {
            await RevokeTokenChainAsync(storedToken, cancellationToken);
            throw new InvalidOperationException("Refresh token has been revoked. Possible token reuse detected.");
        }

        if (storedToken.IsExpired)
            throw new InvalidOperationException("Refresh token has expired");

        var applicationUser = await _userManager.FindByIdAsync(storedToken.UserId.ToString());

        if (applicationUser == null)
            throw new InvalidOperationException("User not found");

        if (!applicationUser.IsActive)
            throw new InvalidOperationException("User is not active");

        var (newRefreshTokenEntity, plainNewRefreshToken) = await CreateRefreshTokenAsync(storedToken.UserId, cancellationToken);

        storedToken.MarkAsRotated(newRefreshTokenEntity.TokenHash);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = await GenerateAccessTokenAsync(applicationUser, cancellationToken);

        return (accessToken, plainNewRefreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be empty", nameof(refreshToken));

        var tokenHash = TokenHasher.Hash(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken == null)
            return;

        if (!storedToken.IsRevoked)
        {
            storedToken.Revoke();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        if (activeTokens.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<string> GenerateAccessTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("userId", user.Id.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    private async Task<(RefreshToken Entity, string PlainToken)> CreateRefreshTokenAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var plainToken = RefreshToken.GenerateToken();
        var tokenHash = TokenHasher.Hash(plainToken);
        var expiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetimeDays);

        var refreshToken = RefreshToken.CreateWithHash(userId, tokenHash, expiresAt);

        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (refreshToken, plainToken);
    }

    private async Task RevokeTokenChainAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        var allUserTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == token.UserId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var userToken in allUserTokens)
        {
            userToken.Revoke();
        }

        if (allUserTokens.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
