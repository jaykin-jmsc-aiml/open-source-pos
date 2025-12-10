using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiquorPOS.Services.Identity.Application.Commands.AssignUserRoles;

public sealed class AssignUserRolesCommandHandler : IRequestHandler<AssignUserRolesCommand, AssignUserRolesCommandResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly ILogger<AssignUserRolesCommandHandler> _logger;

    public AssignUserRolesCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        LiquorPOSIdentityDbContext dbContext,
        ILogger<AssignUserRolesCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AssignUserRolesCommandResponse> Handle(AssignUserRolesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.UserId == Guid.Empty)
            {
                _logger.LogWarning("Assign roles validation failed: User ID is empty");
                return new AssignUserRolesCommandResponse(false, "User ID is required");
            }

            if (request.Roles == null || !request.Roles.Any())
            {
                _logger.LogWarning("Assign roles validation failed: No roles specified");
                return new AssignUserRolesCommandResponse(false, "At least one role must be specified");
            }

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                _logger.LogWarning("User not found with ID {UserId}", request.UserId);
                return new AssignUserRolesCommandResponse(false, "User not found");
            }

            // Validate that all roles exist
            var invalidRoles = new List<string>();
            foreach (var roleName in request.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    invalidRoles.Add(roleName);
                }
            }

            if (invalidRoles.Any())
            {
                _logger.LogWarning("Invalid roles specified: {InvalidRoles}", string.Join(", ", invalidRoles));
                return new AssignUserRolesCommandResponse(false, $"Invalid roles: {string.Join(", ", invalidRoles)}");
            }

            // Get current user roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRolesList = currentRoles.ToList();

            // Determine roles to add and remove
            var rolesToAdd = request.Roles.Except(currentRolesList).ToList();
            var rolesToRemove = currentRolesList.Except(request.Roles).ToList();

            // Add new roles
            IdentityResult addResult = IdentityResult.Success;
            if (rolesToAdd.Any())
            {
                addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    _logger.LogError("Failed to add roles {Roles} to user {UserId}: {Errors}", 
                        string.Join(", ", rolesToAdd), request.UserId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    return new AssignUserRolesCommandResponse(false, "Failed to assign roles");
                }
            }

            // Remove old roles
            IdentityResult removeResult = IdentityResult.Success;
            if (rolesToRemove.Any())
            {
                removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    // If removing roles failed, try to rollback any added roles
                    if (rolesToAdd.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToAdd);
                    }
                    
                    _logger.LogError("Failed to remove roles {Roles} from user {UserId}: {Errors}", 
                        string.Join(", ", rolesToRemove), request.UserId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    return new AssignUserRolesCommandResponse(false, "Failed to remove roles");
                }
            }

            // Create audit log for role assignment
            var changes = $"Roles assigned: {string.Join(", ", request.Roles)}";
            var auditLog = AuditLog.Create(
                "UserRolesAssigned",
                nameof(ApplicationUser),
                user.Id,
                user.Id,
                changes);

            await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Roles {Roles} assigned successfully to user {UserId}", 
                string.Join(", ", request.Roles), request.UserId);

            return new AssignUserRolesCommandResponse(true, "Roles assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while assigning roles to user {UserId}", request.UserId);
            return new AssignUserRolesCommandResponse(false, "An error occurred while assigning roles");
        }
    }
}