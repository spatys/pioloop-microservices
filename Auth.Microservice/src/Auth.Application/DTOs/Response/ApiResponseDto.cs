namespace Auth.Application.DTOs.Response;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public Dictionary<string, string>? FieldErrors { get; set; }

    public static ApiResponseDto<T> FromSuccess(T data)
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponseDto<T> Error(string message)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            FieldErrors = new Dictionary<string, string> { { "global", message } }
        };
    }

    public static ApiResponseDto<T> ValidationError(Dictionary<string, string> errors)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            FieldErrors = errors
        };
    }
}
