namespace Email.Application.DTOs;

public record ApiResponseDto<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public object? Errors { get; init; }
    public string? ErrorType { get; init; }
    public object? Details { get; init; }

    public static ApiResponseDto<T> SuccessResponse(string message, T? data = default)
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponseDto<T> ErrorResponse(string message, string errorType = "Error", object? errors = null, object? details = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            ErrorType = errorType,
            Errors = errors,
            Details = details
        };
    }

    public static ApiResponseDto<T> ValidationErrorResponse(string message, object errors, object? details = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            ErrorType = "ValidationError",
            Errors = errors,
            Details = details
        };
    }

    public static ApiResponseDto<T> InternalErrorResponse(string message, object? details = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            ErrorType = "InternalError",
            Details = details
        };
    }
}
