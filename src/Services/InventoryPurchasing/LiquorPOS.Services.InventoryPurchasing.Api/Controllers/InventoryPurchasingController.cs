using Microsoft.AspNetCore.Mvc;

namespace LiquorPOS.Services.InventoryPurchasing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryPurchasingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("InventoryPurchasing Service is running");
    }
}
