using System;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LiquorPOS.Services.Identity.Infrastructure.Persistence;

public sealed class LiquorPOSIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public LiquorPOSIdentityDbContext(DbContextOptions<LiquorPOSIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(LiquorPOSIdentityDbContext).Assembly);
    }
}
