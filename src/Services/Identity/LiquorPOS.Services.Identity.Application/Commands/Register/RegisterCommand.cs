using LiquorPOS.Services.Identity.Application.Dtos;
using MediatR;

namespace LiquorPOS.Services.Identity.Application.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? PhoneNumber = null,
    string[]? Roles = null) : IRequest<RegisterCommandResponse>;

public sealed record RegisterCommandResponse(
    bool Success,
    string? Message,
    AuthResponse? Data);
