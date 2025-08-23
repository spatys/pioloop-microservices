using Microsoft.AspNetCore.Mvc;

namespace Property.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint for monitoring
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "Property Microservice",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
