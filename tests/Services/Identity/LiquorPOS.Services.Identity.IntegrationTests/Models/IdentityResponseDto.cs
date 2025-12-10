namespace LiquorPOS.Services.Identity.IntegrationTests;

public sealed record IdentityResponseDto<T>(
    bool Success,
    string? Message,
    T? Data);
