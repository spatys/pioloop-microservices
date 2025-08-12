namespace ApiGateway.Models;

/// <summary>
/// Modèle de réponse API standardisé pour toutes les réponses de l'API Gateway
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indique si l'opération a réussi
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message descriptif de la réponse
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Données de la réponse
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Erreurs détaillées (si applicable)
    /// </summary>
    public object? Errors { get; set; }

    /// <summary>
    /// Type d'erreur (si applicable)
    /// </summary>
    public string? ErrorType { get; set; }

    /// <summary>
    /// Détails supplémentaires (si applicable)
    /// </summary>
    public object? Details { get; set; }

    /// <summary>
    /// Timestamp de la réponse
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Crée une réponse de succès
    /// </summary>
    public static ApiResponse<T> SuccessResponse(string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Crée une réponse d'erreur
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, string errorType = "Error", object? errors = null, object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorType = errorType,
            Errors = errors,
            Details = details
        };
    }

    /// <summary>
    /// Crée une réponse d'erreur de validation
    /// </summary>
    public static ApiResponse<T> ValidationErrorResponse(string message, object errors, object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorType = "ValidationError",
            Errors = errors,
            Details = details
        };
    }

    /// <summary>
    /// Crée une réponse d'erreur interne
    /// </summary>
    public static ApiResponse<T> InternalErrorResponse(string message, object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorType = "InternalError",
            Details = details
        };
    }
}
