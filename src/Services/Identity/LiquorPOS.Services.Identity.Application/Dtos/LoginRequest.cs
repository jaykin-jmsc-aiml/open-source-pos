namespace LiquorPOS.Services.Identity.Application.Dtos;

public sealed record LoginRequest(
    string Email,
    string Password);
