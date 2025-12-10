using Microsoft.AspNetCore.Identity;

namespace LiquorPOS.Services.Identity.Domain.Entities;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }

    public static ApplicationRole FromDomain(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        return new ApplicationRole
        {
            Id = role.Id == Guid.Empty ? Guid.NewGuid() : role.Id,
            Name = role.Name,
            NormalizedName = role.Name.ToUpperInvariant(),
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            IsActive = role.IsActive
        };
    }
}