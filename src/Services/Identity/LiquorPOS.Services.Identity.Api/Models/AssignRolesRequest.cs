namespace LiquorPOS.Services.Identity.Api.Models;

public sealed record AssignRolesRequest(
    IReadOnlyCollection<string> Roles);