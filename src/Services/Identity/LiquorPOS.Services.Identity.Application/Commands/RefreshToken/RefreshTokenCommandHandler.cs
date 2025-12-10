using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Domain.Services;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiquorPOS.Services.Identity.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenCommandResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IJwtTokenService jwtTokenService,
        UserManager<ApplicationUser> userManager,
        LiquorPOSIdentityDbContext dbContext,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Refresh token validation failed: Token is empty");
                return new RefreshTokenCommandResponse(false, "Refresh token is required", null);
            }

            var tokenHash = TokenHasher.Hash(request.RefreshToken);
            var storedToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

            if (storedToken == null)
            {
                _logger.LogWarning("Refresh token validation failed: Token not found in database");
                return new RefreshTokenCommandResponse(false, "Invalid refresh token", null);
            }

            if (storedToken.IsRevoked)
            {
                _logger.LogWarning("Refresh token validation failed: Token is revoked");
                await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
                return new RefreshTokenCommandResponse(false, "Refresh token has been revoked. Please login again.", null);
            }

            if (storedToken.IsExpired)
            {
                _logger.LogWarning("Refresh token validation failed: Token has expired");
                return new RefreshTokenCommandResponse(false, "Refresh token has expired. Please login again.", null);
            }

            var applicationUser = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (applicationUser == null)
            {
                _logger.LogWarning("Refresh token validation failed: User not found for token");
                return new RefreshTokenCommandResponse(false, "User not found", null);
            }

            if (!applicationUser.IsActive)
            {
                _logger.LogWarning("Refresh token validation failed: User is inactive");
                return new RefreshTokenCommandResponse(false, "User account is inactive", null);
            }

            var (accessToken, refreshToken) = await _jwtTokenService.RefreshTokensAsync(request.RefreshToken, cancellationToken);

            // Create audit log for token refresh
            var auditLog = AuditLog.Create(
                "RefreshTokenUsed",
                "RefreshToken",
                storedToken.Id,
                storedToken.UserId,
                "User refreshed access token");

            await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = new AuthResponse(
                accessToken,
                refreshToken,
                15,
                applicationUser.Id,
                applicationUser.Email!,
                applicationUser.FirstName,
                applicationUser.LastName);

            _logger.LogInformation("Token refreshed successfully for user {UserId}", storedToken.UserId);
            return new RefreshTokenCommandResponse(true, "Token refreshed successfully", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during token refresh");
            return new RefreshTokenCommandResponse(false, "An error occurred during token refresh", null);
        }
    }
}