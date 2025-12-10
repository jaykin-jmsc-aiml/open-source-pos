using LiquorPOS.Services.Identity.Application.Dtos;
using MediatR;

namespace LiquorPOS.Services.Identity.Application.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<LoginCommandResponse>;

public sealed record LoginCommandResponse(
    bool Success,
    string? Message,
    AuthResponse? Data);
