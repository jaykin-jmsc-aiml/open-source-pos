using LiquorPOS.Services.Identity.Application.Dtos;
using MediatR;

namespace LiquorPOS.Services.Identity.Application.Queries;

public sealed record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null) : IRequest<GetUsersQueryResponse>;

public sealed record GetUsersQueryResponse(
    bool Success,
    string? Message,
    PagedResult<UserListDto>? Data);