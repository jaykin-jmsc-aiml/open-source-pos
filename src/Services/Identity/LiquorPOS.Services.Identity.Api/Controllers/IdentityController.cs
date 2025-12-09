using Microsoft.AspNetCore.Mvc;

namespace LiquorPOS.Services.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Identity Service is running");
    }
}
