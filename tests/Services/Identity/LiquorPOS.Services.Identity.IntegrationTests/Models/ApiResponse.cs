namespace LiquorPOS.Services.Identity.IntegrationTests.Models;

public sealed record ApiResponse<T>(
    bool Success,
    string? Message,
    T? Data);