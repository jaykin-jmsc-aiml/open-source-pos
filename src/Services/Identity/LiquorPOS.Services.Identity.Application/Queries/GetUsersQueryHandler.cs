using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiquorPOS.Services.Identity.Application.Queries;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, GetUsersQueryResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        UserManager<ApplicationUser> userManager,
        LiquorPOSIdentityDbContext dbContext,
        ILogger<GetUsersQueryHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetUsersQueryResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _userManager.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(u => 
                    u.Email!.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm));
            }

            // Apply active status filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            // Apply pagination and ordering
            var users = await query
                .OrderBy(u => u.Email)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var userDtos = new List<UserListDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                var userDto = new UserListDto(
                    user.Id,
                    user.Email!,
                    user.FirstName,
                    user.LastName,
                    user.IsActive,
                    user.CreatedAt,
                    roles.ToList().AsReadOnly());

                userDtos.Add(userDto);
            }

            var pagedResult = new PagedResult<UserListDto>(
                userDtos.AsReadOnly(),
                request.PageNumber,
                request.PageSize,
                totalCount,
                totalPages);

            _logger.LogInformation("Retrieved {Count} users (page {PageNumber} of {TotalPages})", 
                userDtos.Count, request.PageNumber, totalPages);

            return new GetUsersQueryResponse(true, "Users retrieved successfully", pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving users");
            return new GetUsersQueryResponse(false, "An error occurred while retrieving users", null);
        }
    }
}