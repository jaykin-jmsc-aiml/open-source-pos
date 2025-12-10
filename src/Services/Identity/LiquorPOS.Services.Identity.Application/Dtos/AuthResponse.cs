namespace LiquorPOS.Services.Identity.Application.Dtos;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName);
