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

    /// <summary>
    /// Obtient les informations détaillées de l'API Gateway
    /// </summary>
    /// <returns>Informations détaillées du service</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public IActionResult Info()
    {
        var info = new
        {
            Service = "Pioloop API Gateway",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            Timestamp = DateTime.UtcNow,
            Microservices = new[]
            {
                new { Name = "Auth Service", Url = "http://localhost:5001", Status = "Available" },
                new { Name = "Email Service", Url = "http://localhost:5002", Status = "Available" }
            },
            Features = new[]
            {
                "JWT Authentication",
                "Request/Response Logging",
                "Error Handling",
                "Swagger Aggregation",
                "Rate Limiting",
                "CORS Support"
            },
            Endpoints = new[]
            {
                "GET /api/health - Health check",
                "GET /api/health/info - Service information",
                "GET /swagger - Swagger documentation",
                "POST /api/auth/login - User login",
                "POST /api/auth/register - User registration",
                "POST /api/email/send-email-verification - Send email verification"
            }
        };

        return Ok(ApiResponse<object>.SuccessResponse("API Gateway Information", info));
    }
}
