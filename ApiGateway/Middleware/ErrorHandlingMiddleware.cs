using System.Net;
using System.Text.Json;
using ApiGateway.Models;

namespace ApiGateway.Middleware;

/// <summary>
/// Middleware pour la gestion globale des erreurs
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur non gérée s'est produite: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            UnauthorizedAccessException => new ApiResponse<object>
            {
                Success = false,
                Message = "Accès non autorisé",
                ErrorType = "Unauthorized",
                Details = new { error = "Token JWT manquant ou invalide" }
            },
            ArgumentException => new ApiResponse<object>
            {
                Success = false,
                Message = "Paramètres invalides",
                ErrorType = "ValidationError",
                Details = new { error = exception.Message }
            },
            HttpRequestException => new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur de communication avec le microservice",
                ErrorType = "ServiceUnavailable",
                Details = new { error = "Le service demandé n'est pas disponible" }
            },
            _ => new ApiResponse<object>
            {
                Success = false,
                Message = "Une erreur interne s'est produite",
                ErrorType = "InternalError",
                Details = new { error = "Erreur serveur inattendue" }
            }
        };

        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            HttpRequestException => (int)HttpStatusCode.ServiceUnavailable,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
