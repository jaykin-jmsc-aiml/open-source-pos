using LiquorPOS.Services.Identity.Domain.Entities;

namespace LiquorPOS.Services.Identity.UnitTests.TestHelpers.Builders;

public class AuditLogBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _entityId = Guid.NewGuid();
    private string _action = "TestAction";
    private string _entityType = "TestEntity";
    private Guid? _userId = null;
    private string? _changes = "Test changes";
    private string? _oldValues = null;
    private string? _newValues = null;
    private DateTime _createdAt = DateTime.UtcNow;

    public AuditLogBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public AuditLogBuilder ForEntity(Guid entityId)
    {
        _entityId = entityId;
        return this;
    }

    public AuditLogBuilder WithAction(string action)
    {
        _action = action;
        return this;
    }

    public AuditLogBuilder WithEntityType(string entityType)
    {
        _entityType = entityType;
        return this;
    }

    public AuditLogBuilder ForUser(Guid? userId)
    {
        _userId = userId;
        return this;
    }

    public AuditLogBuilder WithChanges(string? changes)
    {
        _changes = changes;
        return this;
    }

    public AuditLogBuilder WithOldValues(string? oldValues)
    {
        _oldValues = oldValues;
        return this;
    }

    public AuditLogBuilder WithNewValues(string? newValues)
    {
        _newValues = newValues;
        return this;
    }

    public AuditLogBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public AuditLog Build()
    {
        return AuditLog.Create(
            _action,
            _entityType,
            _entityId,
            _userId,
            _changes,
            _oldValues,
            _newValues);
    }
}