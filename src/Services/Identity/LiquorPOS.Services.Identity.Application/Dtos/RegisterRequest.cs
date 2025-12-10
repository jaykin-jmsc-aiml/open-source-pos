namespace LiquorPOS.Services.Identity.Application.Dtos;

public sealed record RegisterRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? PhoneNumber = null,
    string[]? Roles = null);
