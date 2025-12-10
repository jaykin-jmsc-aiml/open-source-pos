using System.Collections.Generic;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiquorPOS.Services.Identity.Infrastructure.Persistence.Configurations;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("AspNetRoles");

        builder.Property(r => r.Description)
            .HasMaxLength(512);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.LastModifiedAt);

        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RolePermissions",
                j => j.HasOne<Permission>()
                      .WithMany()
                      .HasForeignKey("PermissionId")
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<ApplicationRole>()
                      .WithMany()
                      .HasForeignKey("RoleId")
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("RoleId", "PermissionId");
                    j.ToTable("RolePermissions");
                });
    }
}
