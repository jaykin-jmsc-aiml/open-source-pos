namespace LiquorPOS.Services.Identity.Application.Dtos;

public sealed record UserListDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyCollection<string> Roles);