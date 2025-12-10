using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Application.Services;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.ValueObjects;
using LiquorPOS.Services.Identity.Infrastructure.Identity;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiquorPOS.Services.Identity.Application.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        LiquorPOSIdentityDbContext dbContext,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = Email.Create(request.Email);
            if (!emailResult.IsSuccess)
            {
                _logger.LogWarning("Login failed: Invalid email format for {Email}", request.Email);
                return new LoginCommandResponse(false, "Invalid credentials", null);
            }

            var applicationUser = await _userManager.FindByEmailAsync(request.Email);
            if (applicationUser == null)
            {
                _logger.LogWarning("Login failed: User not found for {Email}", request.Email);
                return new LoginCommandResponse(false, "Invalid credentials", null);
            }

            if (!applicationUser.IsActive)
            {
                _logger.LogWarning("Login failed: User {Email} is inactive", request.Email);
                return new LoginCommandResponse(false, "User account is inactive", null);
            }

            var passwordValid = _userManager.PasswordHasher.VerifyHashedPassword(
                applicationUser,
                applicationUser.PasswordHash,
                request.Password);

            if (passwordValid == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
                return new LoginCommandResponse(false, "Invalid credentials", null);
            }

            if (applicationUser.LockoutEnabled && applicationUser.LockoutEnd > DateTime.UtcNow)
            {
                _logger.LogWarning("Login failed: User {Email} account is locked", request.Email);
                return new LoginCommandResponse(false, "User account is locked. Please try again later.", null);
            }

            var user = new User
            {
                Id = applicationUser.Id,
                Email = Email.CreateOrThrow(applicationUser.Email!),
                FirstName = applicationUser.FirstName,
                LastName = applicationUser.LastName,
                PhoneNumber = string.IsNullOrWhiteSpace(applicationUser.PhoneNumber)
                    ? null
                    : PhoneNumber.CreateOrThrow(applicationUser.PhoneNumber),
                PasswordHash = PasswordHash.FromHashAndSalt(
                    applicationUser.PasswordHash!,
                    applicationUser.PasswordSalt ?? string.Empty),
                IsActive = applicationUser.IsActive,
                CreatedAt = applicationUser.CreatedAt,
                LastModifiedAt = applicationUser.LastModifiedAt,
                LastLoginAt = applicationUser.LastLoginAt
            };

            var (accessToken, refreshToken) = await _jwtTokenService.GenerateTokensAsync(user, cancellationToken);

            applicationUser.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(applicationUser);

            var auditLog = AuditLog.Create(
                "UserLoggedIn",
                nameof(User),
                applicationUser.Id,
                applicationUser.Id,
                $"User logged in with email {applicationUser.Email}");

            await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = new AuthResponse(
                accessToken,
                refreshToken,
                15,
                user.Id,
                user.Email.Value,
                user.FirstName,
                user.LastName);

            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            return new LoginCommandResponse(true, "Login successful", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for {Email}", request.Email);
            return new LoginCommandResponse(false, "An error occurred during login", null);
        }
    }
}
