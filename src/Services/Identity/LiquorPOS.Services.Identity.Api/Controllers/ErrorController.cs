using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LiquorPOS.Services.Identity.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class ErrorController : ControllerBase
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(IHostEnvironment environment, ILogger<ErrorController> logger)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Route("/error")]
    public IActionResult HandleError()
    {
        var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        _logger.LogError(exception, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError
        };

        if (_environment.IsDevelopment() && exception is not null)
        {
            problem.Detail = exception.Message;
        }

        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;

        return StatusCode(problem.Status.Value, problem);
    }
}
