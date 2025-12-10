using LiquorPOS.Services.Identity.Application.Dtos;
using MediatR;

namespace LiquorPOS.Services.Identity.Application.Queries;

public sealed record GetUserByIdQuery(
    Guid Id) : IRequest<GetUserByIdQueryResponse>;

public sealed record GetUserByIdQueryResponse(
    bool Success,
    string? Message,
    UserDto? Data);