using System;
using LiquorPOS.Services.Identity.Domain.ValueObjects;
using LiquorPOS.Services.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LiquorPOS.Services.Identity.Infrastructure.Security;

public sealed class DomainPasswordHasher : IPasswordHasher<ApplicationUser>
{
    public string HashPassword(ApplicationUser user, string password)
    {
        var passwordHashResult = PasswordHash.Create(password);
        if (!passwordHashResult.IsSuccess || passwordHashResult.Value is null)
        {
            throw new InvalidOperationException(passwordHashResult.Error ?? "Password hashing failed");
        }

        user.PasswordSalt = passwordHashResult.Value.Salt;
        return passwordHashResult.Value.Value;
    }

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        if (string.IsNullOrWhiteSpace(user.PasswordSalt))
        {
            return PasswordVerificationResult.Failed;
        }

        var passwordHash = PasswordHash.FromHashAndSalt(hashedPassword, user.PasswordSalt);
        return passwordHash.Verify(providedPassword)
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
    }
}
