using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.Identity.Domain.Entities;

public sealed class Permission : Entity<Guid>
{
    public string Name { get; set; } = null!;
    public string Scope { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    private Permission() { }

    private Permission(string name, string scope, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Scope = scope;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static Permission Create(string name, string scope, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(scope))
            throw new ArgumentException("Permission scope cannot be empty", nameof(scope));

        return new Permission(name, scope, description);
    }
}
