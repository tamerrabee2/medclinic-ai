using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        version = "1.0.0",
        service = "MedClinic AI API"
    });
}
