using Microsoft.AspNetCore.Mvc;

namespace LiquorPOS.Services.ReportingAnalytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportingAnalyticsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("ReportingAnalytics Service is running");
    }
}
