using LiquorPOS.Services.Identity.Domain.Services;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiquorPOS.Services.Identity.Application.Commands.RevokeToken;

public sealed class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, RevokeTokenCommandResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly ILogger<RevokeTokenCommandHandler> _logger;

    public RevokeTokenCommandHandler(
        IJwtTokenService jwtTokenService,
        UserManager<ApplicationUser> userManager,
        LiquorPOSIdentityDbContext dbContext,
        ILogger<RevokeTokenCommandHandler> logger)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RevokeTokenCommandResponse> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Revoke token validation failed: Token is empty");
                return new RevokeTokenCommandResponse(false, "Refresh token is required");
            }

            var tokenHash = TokenHasher.Hash(request.RefreshToken);
            var storedToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

            if (storedToken == null)
            {
                _logger.LogWarning("Revoke token validation failed: Token not found in database");
                return new RevokeTokenCommandResponse(false, "Invalid refresh token");
            }

            if (storedToken.IsRevoked)
            {
                _logger.LogInformation("Token is already revoked for user {UserId}", storedToken.UserId);
                return new RevokeTokenCommandResponse(true, "Token already revoked");
            }

            // Revoke the token
            await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

            // Create audit log for token revocation
            var auditLog = AuditLog.Create(
                "RefreshTokenRevoked",
                "RefreshToken",
                storedToken.Id,
                storedToken.UserId,
                $"Refresh token revoked at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

            await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token revoked successfully for user {UserId}", storedToken.UserId);
            return new RevokeTokenCommandResponse(true, "Token revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during token revocation");
            return new RevokeTokenCommandResponse(false, "An error occurred during token revocation");
        }
    }
}