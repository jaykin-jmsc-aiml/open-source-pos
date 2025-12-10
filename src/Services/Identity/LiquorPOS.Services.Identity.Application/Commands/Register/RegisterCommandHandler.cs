using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Application.Services;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.ValueObjects;
using LiquorPOS.Services.Identity.Infrastructure.Identity;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LiquorPOS.Services.Identity.Application.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterCommandResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IJwtTokenService jwtTokenService,
        LiquorPOSIdentityDbContext dbContext,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RegisterCommandResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = Email.Create(request.Email);
            if (!emailResult.IsSuccess)
            {
                _logger.LogWarning("Registration failed: Invalid email format for {Email}", request.Email);
                return new RegisterCommandResponse(false, emailResult.Error, null);
            }

            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already registered", request.Email);
                return new RegisterCommandResponse(false, "Email already registered", null);
            }

            var passwordHashResult = PasswordHash.Create(request.Password);
            if (!passwordHashResult.IsSuccess)
            {
                _logger.LogWarning("Registration failed: Invalid password for {Email}", request.Email);
                return new RegisterCommandResponse(false, passwordHashResult.Error, null);
            }

            var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : PhoneNumber.CreateOrThrow(request.PhoneNumber);

            var user = User.Create(
                emailResult.Value!,
                request.FirstName,
                request.LastName,
                passwordHashResult.Value!,
                phoneNumber);

            var applicationUser = ApplicationUser.FromDomain(user);
            applicationUser.PasswordHash = _userManager.PasswordHasher.HashPassword(applicationUser, request.Password);

            var result = await _userManager.CreateAsync(applicationUser);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, errors);
                return new RegisterCommandResponse(false, $"Registration failed: {errors}", null);
            }

            var rolesToAssign = request.Roles ?? new[] { "Manager", "Cashier" };
            foreach (var roleName in rolesToAssign)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(applicationUser, roleName);
                    _logger.LogInformation("Assigned role {Role} to user {Email}", roleName, request.Email);
                }
                else
                {
                    _logger.LogWarning("Role {Role} does not exist during registration for {Email}", roleName, request.Email);
                }
            }

            var (accessToken, refreshToken) = await _jwtTokenService.GenerateTokensAsync(user, cancellationToken);

            var auditLog = AuditLog.Create(
                "UserRegistered",
                nameof(User),
                user.Id,
                user.Id,
                $"User registered with email {user.Email.Value}");

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

            _logger.LogInformation("User {Email} registered successfully", request.Email);
            return new RegisterCommandResponse(true, "Registration successful", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during registration for {Email}", request.Email);
            return new RegisterCommandResponse(false, "An error occurred during registration", null);
        }
    }
}
