using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.Entities;

namespace LiquorPOS.Services.Identity.UnitTests.Entities;

public class PermissionTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var permission = Permission.Create("read_users", "users", "Allows reading user data");

        permission.Should().NotBeNull();
        permission.Name.Should().Be("read_users");
        permission.Scope.Should().Be("users");
        permission.Description.Should().Be("Allows reading user data");
        permission.Id.Should().NotBe(Guid.Empty);
        permission.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithoutDescription_ShouldSucceed()
    {
        var permission = Permission.Create("delete_users", "users");

        permission.Should().NotBeNull();
        permission.Name.Should().Be("delete_users");
        permission.Scope.Should().Be("users");
        permission.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        var action = () => Permission.Create("", "users");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentException()
    {
        var action = () => Permission.Create(null!, "users");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        var action = () => Permission.Create("   ", "users");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyScope_ShouldThrowArgumentException()
    {
        var action = () => Permission.Create("read_users", "");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullScope_ShouldThrowArgumentException()
    {
        var action = () => Permission.Create("read_users", null!);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceScope_ShouldThrowArgumentException()
    {
        var action = () => Permission.Create("read_users", "   ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var permission1 = Permission.Create("read_users", "users");
        var permission2 = Permission.Create("read_users", "users");

        permission1.Id.Should().NotBe(permission2.Id);
    }
}
