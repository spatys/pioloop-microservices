using Microsoft.AspNetCore.Mvc;

namespace Email.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", service = "email-microservice", timestamp = DateTime.UtcNow });
    }
}
