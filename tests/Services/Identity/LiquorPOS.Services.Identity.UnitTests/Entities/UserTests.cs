using FluentAssertions;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Domain.ValueObjects;

namespace LiquorPOS.Services.Identity.UnitTests.Entities;

public class UserTests
{
    private readonly Email _email = Email.CreateOrThrow("user@example.com");
    private readonly string _firstName = "John";
    private readonly string _lastName = "Doe";
    private readonly PasswordHash _passwordHash = PasswordHash.CreateOrThrow("ValidPassword123!");

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        user.Should().NotBeNull();
        user.Email.Should().Be(_email);
        user.FirstName.Should().Be(_firstName);
        user.LastName.Should().Be(_lastName);
        user.PasswordHash.Should().Be(_passwordHash);
        user.PhoneNumber.Should().BeNull();
        user.IsActive.Should().BeTrue();
        user.Id.Should().NotBe(Guid.Empty);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.LastModifiedAt.Should().BeNull();
        user.LastLoginAt.Should().BeNull();
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithPhoneNumber_ShouldSucceed()
    {
        var phoneNumber = PhoneNumber.CreateOrThrow("1234567890");
        var user = User.Create(_email, _firstName, _lastName, _passwordHash, phoneNumber);

        user.PhoneNumber.Should().Be(phoneNumber);
    }

    [Fact]
    public void Create_WithNullEmail_ShouldThrowArgumentNullException()
    {
        var action = () => User.Create(null!, _firstName, _lastName, _passwordHash);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        var action = () => User.Create(_email, "", _lastName, _passwordHash);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyLastName_ShouldThrowArgumentException()
    {
        var action = () => User.Create(_email, _firstName, "", _passwordHash);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullPasswordHash_ShouldThrowArgumentNullException()
    {
        var action = () => User.Create(_email, _firstName, _lastName, null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AssignRole_WithValidRole_ShouldSucceed()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        var role = Role.Create("Administrator");

        user.AssignRole(role);

        user.Roles.Should().ContainSingle();
        user.Roles.First().Should().Be(role);
        user.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void AssignRole_WithDuplicateRole_ShouldNotAddDuplicate()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        var role = Role.Create("Administrator");

        user.AssignRole(role);
        user.AssignRole(role);

        user.Roles.Count.Should().Be(1);
    }

    [Fact]
    public void AssignRole_WithNullRole_ShouldThrowArgumentNullException()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        var action = () => user.AssignRole(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AssignRole_MultipleRoles_ShouldSucceed()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        var role1 = Role.Create("Administrator");
        var role2 = Role.Create("User");

        user.AssignRole(role1);
        user.AssignRole(role2);

        user.Roles.Count.Should().Be(2);
        user.Roles.Should().Contain(role1);
        user.Roles.Should().Contain(role2);
    }

    [Fact]
    public void RemoveRole_WithExistingRole_ShouldSucceed()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        var role = Role.Create("Administrator");
        user.AssignRole(role);

        user.RemoveRole(role);

        user.Roles.Should().BeEmpty();
        user.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemoveRole_WithNonExistingRole_ShouldNotThrow()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        var role = Role.Create("Administrator");

        var action = () => user.RemoveRole(role);

        action.Should().NotThrow();
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_WithNullRole_ShouldThrowArgumentNullException()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        var action = () => user.RemoveRole(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveRoleById_WithExistingRoleId_ShouldSucceed()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        var role = Role.Create("Administrator");
        user.AssignRole(role);

        user.RemoveRoleById(role.Id);

        user.Roles.Should().BeEmpty();
        user.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemoveRoleById_WithNonExistingRoleId_ShouldNotThrow()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        var action = () => user.RemoveRoleById(Guid.NewGuid());

        action.Should().NotThrow();
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void HasRole_WithExistingRoleId_ShouldReturnTrue()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        var role = Role.Create("Administrator");
        user.AssignRole(role);

        var result = user.HasRole(role.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithNonExistingRoleId_ShouldReturnFalse()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        var result = user.HasRole(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldSucceed()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var phoneNumber = PhoneNumber.CreateOrThrow("1234567890");

        user.UpdateProfile(newFirstName, newLastName, phoneNumber);

        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.PhoneNumber.Should().Be(phoneNumber);
        user.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateProfile_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        var action = () => user.UpdateProfile("", _lastName);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_WithEmptyLastName_ShouldThrowArgumentException()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        var action = () => user.UpdateProfile(_firstName, "");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdatePassword_WithNewPasswordHash_ShouldSucceed()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        var newPasswordHash = PasswordHash.CreateOrThrow("NewValidPassword123!");

        user.UpdatePassword(newPasswordHash);

        user.PasswordHash.Should().Be(newPasswordHash);
        user.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePassword_WithNullPasswordHash_ShouldThrowArgumentNullException()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        var action = () => user.UpdatePassword(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetLastLogin_ShouldUpdateLastLoginAt()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        user.SetLastLogin();

        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);

        user.Deactivate();

        user.IsActive.Should().BeFalse();
        user.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var user = User.Create(_email, _firstName, _lastName, _passwordHash);
        user.Deactivate();

        user.Activate();

        user.IsActive.Should().BeTrue();
        user.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var user1 = User.Create(_email, _firstName, _lastName, _passwordHash);
        var user2 = User.Create(_email, _firstName, _lastName, _passwordHash);

        user1.Id.Should().NotBe(user2.Id);
    }
}
