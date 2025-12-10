using System;
using System.Collections.Generic;
using LiquorPOS.Services.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LiquorPOS.Services.Identity.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? PasswordSalt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public static ApplicationUser FromDomain(User user, string? securityStamp = null)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return new ApplicationUser
        {
            Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id,
            UserName = user.Email.Value,
            NormalizedUserName = user.Email.Value.ToUpperInvariant(),
            Email = user.Email.Value,
            NormalizedEmail = user.Email.Value.ToUpperInvariant(),
            EmailConfirmed = true,
            PhoneNumber = user.PhoneNumber?.Value,
            PhoneNumberConfirmed = user.PhoneNumber is not null,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastModifiedAt = user.LastModifiedAt,
            LastLoginAt = user.LastLoginAt,
            PasswordHash = user.PasswordHash.Value,
            PasswordSalt = user.PasswordHash.Salt,
            SecurityStamp = securityStamp ?? Guid.NewGuid().ToString("D"),
            TwoFactorEnabled = false,
            LockoutEnabled = false
        };
    }
}
