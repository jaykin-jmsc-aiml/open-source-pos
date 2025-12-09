using Microsoft.AspNetCore.Mvc;

namespace LiquorPOS.Services.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Catalog Service is running");
    }
}
