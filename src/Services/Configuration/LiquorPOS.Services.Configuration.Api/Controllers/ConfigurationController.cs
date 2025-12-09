using Microsoft.AspNetCore.Mvc;

namespace LiquorPOS.Services.Configuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Configuration Service is running");
    }
}
