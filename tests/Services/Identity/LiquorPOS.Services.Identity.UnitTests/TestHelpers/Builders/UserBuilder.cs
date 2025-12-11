using LiquorPOS.Services.Identity.Domain.Entities;
using System.Security.Claims;

namespace LiquorPOS.Services.Identity.UnitTests.TestHelpers.Builders;

public class UserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _email = "test@example.com";
    private string _password = "SecurePassword123!";
    private string _firstName = "Test";
    private string _lastName = "User";
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _lastModifiedAt = null;
    private DateTime? _lastLoginAt = null;
    private bool _emailConfirmed = true;
    private string? _phoneNumber = null;
    private bool _phoneNumberConfirmed = false;
    private string? _securityStamp = null;

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public UserBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public UserBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public UserBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public UserBuilder WithLastModifiedAt(DateTime? lastModifiedAt)
    {
        _lastModifiedAt = lastModifiedAt;
        return this;
    }

    public UserBuilder WithLastLoginAt(DateTime? lastLoginAt)
    {
        _lastLoginAt = lastLoginAt;
        return this;
    }

    public UserBuilder WithEmailConfirmed(bool emailConfirmed)
    {
        _emailConfirmed = emailConfirmed;
        return this;
    }

    public UserBuilder WithPhoneNumber(string? phoneNumber)
    {
        _phoneNumber = phoneNumber;
        _phoneNumberConfirmed = !string.IsNullOrEmpty(phoneNumber);
        return this;
    }

    public UserBuilder WithSecurityStamp(string? securityStamp)
    {
        _securityStamp = securityStamp;
        return this;
    }

    public ApplicationUser Build()
    {
        return new ApplicationUser
        {
            Id = _id,
            UserName = _email,
            NormalizedUserName = _email.ToUpperInvariant(),
            Email = _email,
            NormalizedEmail = _email.ToUpperInvariant(),
            EmailConfirmed = _emailConfirmed,
            PhoneNumber = _phoneNumber,
            PhoneNumberConfirmed = _phoneNumberConfirmed,
            FirstName = _firstName,
            LastName = _lastName,
            IsActive = _isActive,
            CreatedAt = _createdAt,
            LastModifiedAt = _lastModifiedAt,
            LastLoginAt = _lastLoginAt,
            PasswordHash = _password, // In real scenario, this would be hashed
            PasswordSalt = "salt", // In real scenario, this would be real salt
            SecurityStamp = _securityStamp ?? Guid.NewGuid().ToString("D"),
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            AccessFailedCount = 0,
            LockoutEnd = null
        };
    }
}