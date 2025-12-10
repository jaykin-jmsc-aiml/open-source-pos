using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.Entities;

namespace LiquorPOS.Services.Identity.UnitTests.Entities;

public class AuditLogTests
{
    [Fact]
    public void Create_WithRequiredData_ShouldSucceed()
    {
        var action = "CREATE";
        var entityType = "User";
        var entityId = Guid.NewGuid();

        var auditLog = AuditLog.Create(action, entityType, entityId);

        auditLog.Should().NotBeNull();
        auditLog.Action.Should().Be(action);
        auditLog.EntityType.Should().Be(entityType);
        auditLog.EntityId.Should().Be(entityId);
        auditLog.UserId.Should().BeNull();
        auditLog.Changes.Should().BeNull();
        auditLog.OldValues.Should().BeNull();
        auditLog.NewValues.Should().BeNull();
        auditLog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        auditLog.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithAllData_ShouldSucceed()
    {
        var action = "UPDATE";
        var entityType = "User";
        var entityId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var changes = "FirstName";
        var oldValues = "John";
        var newValues = "Jane";

        var auditLog = AuditLog.Create(action, entityType, entityId, userId, changes, oldValues, newValues);

        auditLog.Action.Should().Be(action);
        auditLog.EntityType.Should().Be(entityType);
        auditLog.EntityId.Should().Be(entityId);
        auditLog.UserId.Should().Be(userId);
        auditLog.Changes.Should().Be(changes);
        auditLog.OldValues.Should().Be(oldValues);
        auditLog.NewValues.Should().Be(newValues);
    }

    [Fact]
    public void Create_WithEmptyAction_ShouldThrowArgumentException()
    {
        var action = () => AuditLog.Create("", "User", Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullAction_ShouldThrowArgumentException()
    {
        var action = () => AuditLog.Create(null!, "User", Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceAction_ShouldThrowArgumentException()
    {
        var action = () => AuditLog.Create("   ", "User", Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyEntityType_ShouldThrowArgumentException()
    {
        var action = () => AuditLog.Create("CREATE", "", Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullEntityType_ShouldThrowArgumentException()
    {
        var action = () => AuditLog.Create("CREATE", null!, Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceEntityType_ShouldThrowArgumentException()
    {
        var action = () => AuditLog.Create("CREATE", "   ", Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyEntityId_ShouldThrowArgumentException()
    {
        var action = () => AuditLog.Create("CREATE", "User", Guid.Empty);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var auditLog1 = AuditLog.Create("CREATE", "User", Guid.NewGuid());
        var auditLog2 = AuditLog.Create("CREATE", "User", Guid.NewGuid());

        auditLog1.Id.Should().NotBe(auditLog2.Id);
    }

    [Theory]
    [InlineData("CREATE")]
    [InlineData("UPDATE")]
    [InlineData("DELETE")]
    [InlineData("READ")]
    public void Create_WithDifferentActions_ShouldSucceed(string action)
    {
        var auditLog = AuditLog.Create(action, "User", Guid.NewGuid());

        auditLog.Action.Should().Be(action);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Role")]
    [InlineData("Permission")]
    [InlineData("RefreshToken")]
    public void Create_WithDifferentEntityTypes_ShouldSucceed(string entityType)
    {
        var auditLog = AuditLog.Create("CREATE", entityType, Guid.NewGuid());

        auditLog.EntityType.Should().Be(entityType);
    }
}
