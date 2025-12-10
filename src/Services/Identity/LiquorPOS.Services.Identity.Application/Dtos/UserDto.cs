namespace LiquorPOS.Services.Identity.Application.Dtos;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastModifiedAt,
    DateTime? LastLoginAt,
    IReadOnlyCollection<string> Roles);