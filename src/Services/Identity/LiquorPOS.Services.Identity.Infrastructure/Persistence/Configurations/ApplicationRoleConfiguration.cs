using LiquorPOS.Services.Identity.Domain.Entities;
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

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);
    }
}
