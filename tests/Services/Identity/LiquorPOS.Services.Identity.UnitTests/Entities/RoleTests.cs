using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.Entities;

namespace LiquorPOS.Services.Identity.UnitTests.Entities;

public class RoleTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var role = Role.Create("Administrator", "Admin role with full access");

        role.Should().NotBeNull();
        role.Name.Should().Be("Administrator");
        role.Description.Should().Be("Admin role with full access");
        role.Id.Should().NotBe(Guid.Empty);
        role.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        role.LastModifiedAt.Should().BeNull();
        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithoutDescription_ShouldSucceed()
    {
        var role = Role.Create("User");

        role.Should().NotBeNull();
        role.Name.Should().Be("User");
        role.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        var action = () => Role.Create("");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentException()
    {
        var action = () => Role.Create(null!);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        var action = () => Role.Create("   ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ShouldThrowArgumentException()
    {
        var longName = new string('a', 101);
        var action = () => Role.Create(longName);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddPermission_WithValidPermission_ShouldSucceed()
    {
        var role = Role.Create("Administrator");
        var permission = Permission.Create("read_users", "users");

        role.AddPermission(permission);

        role.Permissions.Should().ContainSingle();
        role.Permissions.First().Should().Be(permission);
        role.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddPermission_WithDuplicatePermission_ShouldNotAddDuplicate()
    {
        var role = Role.Create("Administrator");
        var permission = Permission.Create("read_users", "users");

        role.AddPermission(permission);
        role.AddPermission(permission);

        role.Permissions.Count.Should().Be(1);
    }

    [Fact]
    public void AddPermission_WithNullPermission_ShouldThrowArgumentNullException()
    {
        var role = Role.Create("Administrator");

        var action = () => role.AddPermission(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddPermission_MultiplePermissions_ShouldSucceed()
    {
        var role = Role.Create("Administrator");
        var permission1 = Permission.Create("read_users", "users");
        var permission2 = Permission.Create("write_users", "users");
        var permission3 = Permission.Create("delete_users", "users");

        role.AddPermission(permission1);
        role.AddPermission(permission2);
        role.AddPermission(permission3);

        role.Permissions.Count.Should().Be(3);
        role.Permissions.Should().Contain(permission1);
        role.Permissions.Should().Contain(permission2);
        role.Permissions.Should().Contain(permission3);
    }

    [Fact]
    public void RemovePermission_WithExistingPermission_ShouldSucceed()
    {
        var role = Role.Create("Administrator");
        var permission = Permission.Create("read_users", "users");
        role.AddPermission(permission);

        role.RemovePermission(permission);

        role.Permissions.Should().BeEmpty();
        role.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemovePermission_WithNonExistingPermission_ShouldNotThrow()
    {
        var role = Role.Create("Administrator");
        var permission = Permission.Create("read_users", "users");

        var action = () => role.RemovePermission(permission);

        action.Should().NotThrow();
        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void RemovePermission_WithNullPermission_ShouldThrowArgumentNullException()
    {
        var role = Role.Create("Administrator");

        var action = () => role.RemovePermission(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemovePermissionById_WithExistingPermissionId_ShouldSucceed()
    {
        var role = Role.Create("Administrator");
        var permission = Permission.Create("read_users", "users");
        role.AddPermission(permission);

        role.RemovePermissionById(permission.Id);

        role.Permissions.Should().BeEmpty();
        role.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemovePermissionById_WithNonExistingPermissionId_ShouldNotThrow()
    {
        var role = Role.Create("Administrator");

        var action = () => role.RemovePermissionById(Guid.NewGuid());

        action.Should().NotThrow();
        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void HasPermission_WithExistingPermissionId_ShouldReturnTrue()
    {
        var role = Role.Create("Administrator");
        var permission = Permission.Create("read_users", "users");
        role.AddPermission(permission);

        var result = role.HasPermission(permission.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasPermission_WithNonExistingPermissionId_ShouldReturnFalse()
    {
        var role = Role.Create("Administrator");

        var result = role.HasPermission(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var role1 = Role.Create("Administrator");
        var role2 = Role.Create("Administrator");

        role1.Id.Should().NotBe(role2.Id);
    }
}
