using MediatR;

namespace LiquorPOS.Services.Identity.Application.Commands.AssignUserRoles;

public sealed record AssignUserRolesCommand(
    Guid UserId,
    IReadOnlyCollection<string> Roles) : IRequest<AssignUserRolesCommandResponse>;

public sealed record AssignUserRolesCommandResponse(
    bool Success,
    string? Message);