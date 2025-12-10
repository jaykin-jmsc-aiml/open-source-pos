namespace LiquorPOS.Services.Identity.Api.Models;

public sealed record IdentityResponse<T>(
    bool Success,
    string? Message,
    T? Data);
