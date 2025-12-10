using LiquorPOS.Services.Identity.Application.Commands.Login;
using LiquorPOS.Services.Identity.Application.Commands.Register;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Api.Models;
using MediatR;
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
    [ProduceResponseType(typeof(IdentityResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(IdentityResponse<object>), StatusCodes.Status400BadRequest)]
    [ProduceResponseType(typeof(IdentityResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.Information("Registration attempt for email: {Email}", request.Email);

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
                _logger.Warning("Registration failed: {Message}", result.Message);
                return BadRequest(new IdentityResponse<object>(false, result.Message, null!));
            }

            _logger.Information("Registration successful for email: {Email}", request.Email);
            return Ok(new IdentityResponse<AuthResponse>(true, result.Message, result.Data!));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred during registration");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new IdentityResponse<object>(false, "An error occurred during registration", null!));
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login request with credentials</param>
    /// <returns>Auth response with access and refresh tokens</returns>
    /// <response code="200">User logged in successfully</response>
    /// <response code="400">Invalid credentials or user inactive</response>
    /// <response code="500">Server error</response>
    [HttpPost("login")]
    [ProduceResponseType(typeof(IdentityResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(IdentityResponse<object>), StatusCodes.Status400BadRequest)]
    [ProduceResponseType(typeof(IdentityResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.Information("Login attempt for email: {Email}", request.Email);

            var command = new LoginCommand(request.Email, request.Password);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                _logger.Warning("Login failed: {Message}", result.Message);
                return BadRequest(new IdentityResponse<object>(false, result.Message, null!));
            }

            _logger.Information("Login successful for email: {Email}", request.Email);
            return Ok(new IdentityResponse<AuthResponse>(true, result.Message, result.Data!));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred during login");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new IdentityResponse<object>(false, "An error occurred during login", null!));
        }
    }
}
