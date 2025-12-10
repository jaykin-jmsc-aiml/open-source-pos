using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.Identity.Domain.Entities;

public sealed class Role : Entity<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    private readonly List<Permission> _permissions = [];

    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    private Role() { }

    private Role(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static Role Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Role name cannot exceed 100 characters", nameof(name));

        return new Role(name, description);
    }

    public void AddPermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        if (_permissions.Any(p => p.Id == permission.Id))
            return;

        _permissions.Add(permission);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemovePermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        if (_permissions.Remove(permission))
            LastModifiedAt = DateTime.UtcNow;
    }

    public void RemovePermissionById(Guid permissionId)
    {
        var permission = _permissions.FirstOrDefault(p => p.Id == permissionId);
        if (permission != null)
        {
            _permissions.Remove(permission);
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    public bool HasPermission(Guid permissionId) => _permissions.Any(p => p.Id == permissionId);
}
