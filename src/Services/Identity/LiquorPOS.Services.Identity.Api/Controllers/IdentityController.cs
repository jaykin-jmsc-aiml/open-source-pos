using LiquorPOS.Services.Identity.Application.Commands.AssignUserRoles;
using LiquorPOS.Services.Identity.Application.Commands.Login;
using LiquorPOS.Services.Identity.Application.Commands.RefreshToken;
using LiquorPOS.Services.Identity.Application.Commands.Register;
using LiquorPOS.Services.Identity.Application.Commands.RevokeToken;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Application.Queries;
using LiquorPOS.Services.Identity.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace LiquorPOS.Services.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IdentityController> _logger;

    public IdentityController(IMediator mediator, ILogger<IdentityController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration request with user details</param>
    /// <returns>Auth response with access and refresh tokens</returns>
    /// <response code="200">User registered successfully</response>
    /// <response code="400">Validation error or duplicate email</response>
    /// <response code="500">Server error</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(IdentityResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            var command = new RegisterCommand(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password,
                request.PhoneNumber,
                request.Roles);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Registration failed: {Message}", result.Message);
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Registration Failed",
                    detail: result.Message
                );
            }

            _logger.LogInformation("Registration successful for email: {Email}", request.Email);
            return Ok(new IdentityResponse<AuthResponse>(true, result.Message, result.Data!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during registration");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error",
                detail: "An error occurred during registration"
            );
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login request with credentials</param>
    /// <returns>Auth response with access and refresh tokens</returns>
    /// <response code="200">User logged in successfully</response>
    /// <response code="400">Bad request (e.g., inactive/locked account)</response>
    /// <response code="401">Unauthorized (invalid credentials)</response>
    /// <response code="500">Server error</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(IdentityResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var command = new LoginCommand(request.Email, request.Password);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Login failed: {Message}", result.Message);

                if (result.Message == "Invalid credentials")
                {
                    return Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "Unauthorized",
                        detail: result.Message
                    );
                }

                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request",
                    detail: result.Message
                );
            }

            _logger.LogInformation("Login successful for email: {Email}", request.Email);
            return Ok(new IdentityResponse<AuthResponse>(true, result.Message, result.Data!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error",
                detail: "An error occurred during login"
            );
        }
    }

    /// <summary>
    /// Refresh access token using a valid refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New auth response with fresh tokens</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Bad request (e.g., malformed request)</response>
    /// <response code="401">Unauthorized (invalid, expired, or revoked refresh token)</response>
    /// <response code="500">Server error</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(IdentityResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt with refresh token");

            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Token refresh failed: {Message}", result.Message);

                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized",
                    detail: result.Message
                );
            }

            _logger.LogInformation("Token refreshed successfully");
            return Ok(new IdentityResponse<AuthResponse>(true, result.Message, result.Data!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during token refresh");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error",
                detail: "An error occurred during token refresh"
            );
        }
    }

    /// <summary>
    /// Revoke a refresh token, making it unusable
    /// </summary>
    /// <param name="request">Revoke token request</param>
    /// <returns>Operation result</returns>
    /// <response code="200">Token revoked successfully</response>
    /// <response code="400">Invalid refresh token</response>
    /// <response code="500">Server error</response>
    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(IdentityResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Token revocation attempt");

            var command = new RevokeTokenCommand(request.RefreshToken);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Token revocation failed: {Message}", result.Message);
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Revocation Failed",
                    detail: result.Message
                );
            }

            _logger.LogInformation("Token revoked successfully");
            return Ok(new IdentityResponse<object>(true, result.Message, null!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during token revocation");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error",
                detail: "An error occurred during token revocation"
            );
        }
    }

    /// <summary>
    /// Get a list of users with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="searchTerm">Search term for email, first name, or last name</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>Paged result of users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">Unauthorized - requires Admin or Manager role</response>
    /// <response code="500">Server error</response>
    [HttpGet("users")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(IdentityResponse<PagedResult<UserListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Get users request - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}, Active: {IsActive}",
                pageNumber, pageSize, searchTerm, isActive);

            var query = new GetUsersQuery(pageNumber, pageSize, searchTerm, isActive);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Get users failed: {Message}", result.Message);
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Get Users Failed",
                    detail: result.Message
                );
            }

            _logger.LogInformation("Users retrieved successfully");
            return Ok(new IdentityResponse<PagedResult<UserListDto>>(true, result.Message, result.Data!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving users");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error",
                detail: "An error occurred while retrieving users"
            );
        }
    }

    /// <summary>
    /// Get user details by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    /// <response code="200">User retrieved successfully</response>
    /// <response code="401">Unauthorized - requires Admin or Manager role</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("users/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(IdentityResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Get user by ID request: {UserId}", id);

            var query = new GetUserByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Get user failed: {Message}", result.Message);
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
                        detail: result.Message
                    );
                }
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request",
                    detail: result.Message
                );
            }

            _logger.LogInformation("User retrieved successfully");
            return Ok(new IdentityResponse<UserDto>(true, result.Message, result.Data!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user {UserId}", id);
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error",
                detail: "An error occurred while retrieving user"
            );
        }
    }

    /// <summary>
    /// Assign roles to a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="roles">List of roles to assign</param>
    /// <returns>Operation result</returns>
    /// <response code="200">Roles assigned successfully</response>
    /// <response code="400">Invalid user ID, roles, or validation error</response>
    /// <response code="401">Unauthorized - requires Admin or Manager role</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Server error</response>
    [HttpPost("users/{id:guid}/roles")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(IdentityResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignUserRoles(Guid id, [FromBody] string[] roles, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Assign roles request for user {UserId}: {Roles}", id, string.Join(", ", roles));

            var command = new AssignUserRolesCommand(id, roles.ToList().AsReadOnly());
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Assign roles failed: {Message}", result.Message);
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
                        detail: result.Message
                    );
                }
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request",
                    detail: result.Message
                );
            }

            _logger.LogInformation("Roles assigned successfully for user {UserId}", id);
            return Ok(new IdentityResponse<object>(true, result.Message, null!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while assigning roles to user {UserId}", id);
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error",
                detail: "An error occurred while assigning roles"
            );
        }
    }
}
