using LiquorPOS.Services.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiquorPOS.Services.Identity.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.Property(token => token.TokenHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(token => token.ReplacedByTokenHash)
            .HasMaxLength(256);

        builder.Property(token => token.UserId)
            .IsRequired();

        builder.Property(token => token.CreatedAt)
            .IsRequired();

        builder.Property(token => token.ExpiresAt)
            .IsRequired();

        builder.HasIndex(token => token.TokenHash)
            .IsUnique();

        builder.HasIndex(token => token.UserId);
    }
}
