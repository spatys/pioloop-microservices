using Microsoft.AspNetCore.Mvc;

namespace Property.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public ActionResult Get()
    {
        return Ok(new { Status = "Healthy", Service = "Property Microservice", Timestamp = DateTime.UtcNow });
    }
}
