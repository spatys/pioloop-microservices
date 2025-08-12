using Microsoft.AspNetCore.Mvc;
using ApiGateway.Models;

namespace ApiGateway.Controllers;

/// <summary>
/// Contrôleur pour les endpoints de santé et d'information de l'API Gateway
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Vérifie l'état de santé de l'API Gateway
    /// </summary>
    /// <returns>Statut de santé de l'API Gateway</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check demandé");
        
        var healthInfo = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "Pioloop API Gateway",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        };

        return Ok(ApiResponse<object>.SuccessResponse("API Gateway is healthy", healthInfo));
    }


}
