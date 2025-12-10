namespace LiquorPOS.Services.Identity.Api.Models;

public sealed record RevokeTokenRequest(
    string RefreshToken);