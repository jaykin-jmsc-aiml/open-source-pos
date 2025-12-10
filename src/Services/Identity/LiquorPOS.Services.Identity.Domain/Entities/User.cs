using LiquorPOS.BuildingBlocks.Domain;
using LiquorPOS.Services.Identity.Domain.ValueObjects;

namespace LiquorPOS.Services.Identity.Domain.Entities;

public sealed class User : Entity<Guid>
{
    public Email Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public PhoneNumber? PhoneNumber { get; set; }
    public PasswordHash PasswordHash { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    private readonly List<Role> _roles = [];

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private User() { }

    private User(
        Email email,
        string firstName,
        string lastName,
        PasswordHash passwordHash,
        PhoneNumber? phoneNumber = null)
    {
        Id = Guid.NewGuid();
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(
        Email email,
        string firstName,
        string lastName,
        PasswordHash passwordHash,
        PhoneNumber? phoneNumber = null)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        if (passwordHash == null)
            throw new ArgumentNullException(nameof(passwordHash));

        return new User(email, firstName, lastName, passwordHash, phoneNumber);
    }

    public void AssignRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_roles.Any(r => r.Id == role.Id))
            return;

        _roles.Add(role);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_roles.Remove(role))
            LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveRoleById(Guid roleId)
    {
        var role = _roles.FirstOrDefault(r => r.Id == roleId);
        if (role != null)
        {
            _roles.Remove(role);
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    public bool HasRole(Guid roleId) => _roles.Any(r => r.Id == roleId);

    public void UpdateProfile(string firstName, string lastName, PhoneNumber? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(PasswordHash newPasswordHash)
    {
        if (newPasswordHash == null)
            throw new ArgumentNullException(nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }
}
