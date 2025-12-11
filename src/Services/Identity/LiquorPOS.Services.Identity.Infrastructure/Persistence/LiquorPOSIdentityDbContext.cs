using System;
using LiquorPOS.Services.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LiquorPOS.Services.Identity.Infrastructure.Persistence;

public class LiquorPOSIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public LiquorPOSIdentityDbContext()
    {
    }

    public LiquorPOSIdentityDbContext(DbContextOptions<LiquorPOSIdentityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Permission> Permissions => Set<Permission>();
    public virtual DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public virtual DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(LiquorPOSIdentityDbContext).Assembly);
    }
}
