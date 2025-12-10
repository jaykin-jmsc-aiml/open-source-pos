using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.Identity.Domain.Entities;

public sealed class AuditLog : Entity<Guid>
{
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string? Changes { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime CreatedAt { get; set; }

    private AuditLog() { }

    private AuditLog(
        string action,
        string entityType,
        Guid entityId,
        Guid? userId = null,
        string? changes = null,
        string? oldValues = null,
        string? newValues = null)
    {
        Id = Guid.NewGuid();
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        UserId = userId;
        Changes = changes;
        OldValues = oldValues;
        NewValues = newValues;
        CreatedAt = DateTime.UtcNow;
    }

    public static AuditLog Create(
        string action,
        string entityType,
        Guid entityId,
        Guid? userId = null,
        string? changes = null,
        string? oldValues = null,
        string? newValues = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty", nameof(entityType));

        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));

        return new AuditLog(action, entityType, entityId, userId, changes, oldValues, newValues);
    }
}
