using Microsoft.AspNetCore.Http;
using Serilog;
using System.Diagnostics;

namespace ApiGateway.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = Log.ForContext<RequestLoggingMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        _logger.Information("Début de la requête {Method} {Path}", requestMethod, requestPath);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            _logger.Information("Fin de la requête {Method} {Path} - Status: {StatusCode} - Durée: {Elapsed}ms", 
                requestMethod, requestPath, statusCode, elapsed);
        }
    }
}
