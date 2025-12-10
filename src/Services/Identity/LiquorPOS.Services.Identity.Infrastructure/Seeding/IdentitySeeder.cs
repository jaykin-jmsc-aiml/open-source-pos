using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiquorPOS.Services.Identity.Infrastructure.Seeding;

public sealed class IdentitySeeder : IIdentitySeeder
{
    private static readonly (string Name, string Description)[] DefaultRoles = new[]
    {
        ("Admin", "System administrator with full access"),
        ("Manager", "Store manager with elevated permissions"),
        ("Cashier", "Point-of-sale operator")
    };

    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<IdentitySeeder> _logger;
    private readonly IdentitySeedOptions _options;

    public IdentitySeeder(
        LiquorPOSIdentityDbContext dbContext,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<IdentitySeedOptions> options,
        ILogger<IdentitySeeder> logger)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
        _options = options.Value;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        await EnsureRolesAsync(cancellationToken);
        await EnsureAdminUserAsync(cancellationToken);
    }

    private async Task EnsureRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var (name, description) in DefaultRoles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await _roleManager.RoleExistsAsync(name))
            {
                continue;
            }

            var role = new ApplicationRole
            {
                Name = name,
                NormalizedName = name.ToUpperInvariant(),
                Description = description,
                CreatedAt = DateTime.UtcNow,
                ConcurrencyStamp = Guid.NewGuid().ToString("D")
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role '{name}': {errors}");
            }

            _logger.LogInformation("Seeded default role {Role}", name);
        }
    }

    private async Task EnsureAdminUserAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var adminOptions = _options.Admin;
        var email = adminOptions.Email.Trim().ToLowerInvariant();

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                FirstName = adminOptions.FirstName,
                LastName = adminOptions.LastName,
                PhoneNumber = adminOptions.PhoneNumber,
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(adminOptions.PhoneNumber),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var createResult = await _userManager.CreateAsync(user, adminOptions.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }

            _logger.LogInformation("Seeded default admin user {Email}", email);
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var addToRole = await _userManager.AddToRoleAsync(user, "Admin");
            if (!addToRole.Succeeded)
            {
                var errors = string.Join(", ", addToRole.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign admin role: {errors}");
            }
        }
    }
}
