using Microsoft.AspNetCore.Mvc;

namespace LiquorPOS.Services.SalesPOS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesPOSController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("SalesPOS Service is running");
    }
}
