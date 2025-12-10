using MediatR;

namespace LiquorPOS.Services.Identity.Application.Commands.RevokeToken;

public sealed record RevokeTokenCommand(
    string RefreshToken) : IRequest<RevokeTokenCommandResponse>;

public sealed record RevokeTokenCommandResponse(
    bool Success,
    string? Message);