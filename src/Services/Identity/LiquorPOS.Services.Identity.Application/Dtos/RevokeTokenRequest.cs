namespace LiquorPOS.Services.Identity.Application.Dtos;

public sealed record RevokeTokenRequest(
    string RefreshToken);