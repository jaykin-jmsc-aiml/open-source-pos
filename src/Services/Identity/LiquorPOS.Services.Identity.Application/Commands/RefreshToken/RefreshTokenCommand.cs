using LiquorPOS.Services.Identity.Application.Dtos;
using MediatR;

namespace LiquorPOS.Services.Identity.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken) : IRequest<RefreshTokenCommandResponse>;

public sealed record RefreshTokenCommandResponse(
    bool Success,
    string? Message,
    AuthResponse? Data);