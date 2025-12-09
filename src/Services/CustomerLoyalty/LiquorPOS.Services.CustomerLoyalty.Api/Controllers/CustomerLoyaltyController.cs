using Microsoft.AspNetCore.Mvc;

namespace LiquorPOS.Services.CustomerLoyalty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerLoyaltyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("CustomerLoyalty Service is running");
    }
}
