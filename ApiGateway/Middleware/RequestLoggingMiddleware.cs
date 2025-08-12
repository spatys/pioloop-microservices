using System.Diagnostics;
using System.Text;

namespace ApiGateway.Middleware;

/// <summary>
/// Middleware pour logger les requêtes et réponses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();
        
        // Log de la requête entrante
        await LogRequest(context, requestId);
        
        // Capture de la réponse
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;
        
        try
        {
            await _next(context);
            stopwatch.Stop();
            
            // Log de la réponse
            await LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);
            
            // Copie de la réponse vers le stream original
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erreur lors du traitement de la requête {RequestId}: {Message}", requestId, ex.Message);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequest(HttpContext context, string requestId)
    {
        var request = context.Request;
        var requestBody = string.Empty;

        // Capture du body de la requête pour les méthodes POST/PUT/PATCH
        if (request.Method is "POST" or "PUT" or "PATCH")
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        var logMessage = new
        {
            RequestId = requestId,
            Timestamp = DateTime.UtcNow,
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = requestBody,
            ClientIP = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = request.Headers["User-Agent"].ToString()
        };

        _logger.LogInformation("Requête entrante: {@RequestLog}", logMessage);
    }

    private async Task LogResponse(HttpContext context, string requestId, long elapsedMs)
    {
        var response = context.Response;
        var responseBody = string.Empty;

        // Capture du body de la réponse
        if (response.Body is MemoryStream memoryStream)
        {
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true);
            responseBody = await reader.ReadToEndAsync();
            memoryStream.Position = 0;
        }

        var logMessage = new
        {
            RequestId = requestId,
            Timestamp = DateTime.UtcNow,
            StatusCode = response.StatusCode,
            Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = responseBody.Length > 1000 ? responseBody[..1000] + "..." : responseBody,
            ElapsedMs = elapsedMs
        };

        _logger.LogInformation("Réponse sortante: {@ResponseLog}", logMessage);
    }
}
