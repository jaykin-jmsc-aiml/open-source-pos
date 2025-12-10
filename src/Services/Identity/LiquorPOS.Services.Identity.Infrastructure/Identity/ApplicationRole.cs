using System;
using System.Collections.Generic;
using LiquorPOS.Services.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LiquorPOS.Services.Identity.Infrastructure.Identity;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    public static ApplicationRole FromDomain(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var applicationRole = new ApplicationRole
        {
            Id = role.Id == Guid.Empty ? Guid.NewGuid() : role.Id,
            Name = role.Name,
            NormalizedName = role.Name.ToUpperInvariant(),
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            LastModifiedAt = role.LastModifiedAt,
            ConcurrencyStamp = Guid.NewGuid().ToString("D")
        };

        foreach (var permission in role.Permissions)
        {
            applicationRole.Permissions.Add(permission);
        }

        return applicationRole;
    }
}
