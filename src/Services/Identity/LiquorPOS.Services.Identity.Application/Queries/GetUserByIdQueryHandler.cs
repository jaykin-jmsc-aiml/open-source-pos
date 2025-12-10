using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiquorPOS.Services.Identity.Application.Queries;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdQueryResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly LiquorPOSIdentityDbContext _dbContext;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        UserManager<ApplicationUser> userManager,
        LiquorPOSIdentityDbContext dbContext,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetUserByIdQueryResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Id == Guid.Empty)
            {
                _logger.LogWarning("Get user by ID validation failed: User ID is empty");
                return new GetUserByIdQueryResponse(false, "User ID is required", null);
            }

            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                _logger.LogWarning("User not found with ID {UserId}", request.Id);
                return new GetUserByIdQueryResponse(false, "User not found", null);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.IsActive,
                user.CreatedAt,
                user.LastModifiedAt,
                user.LastLoginAt,
                roles.ToList().AsReadOnly());

            _logger.LogInformation("Retrieved user details for {Email} (ID: {UserId})", user.Email, request.Id);
            return new GetUserByIdQueryResponse(true, "User retrieved successfully", userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user {UserId}", request.Id);
            return new GetUserByIdQueryResponse(false, "An error occurred while retrieving user", null);
        }
    }
}