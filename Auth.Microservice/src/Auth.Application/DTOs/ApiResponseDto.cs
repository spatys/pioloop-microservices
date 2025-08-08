namespace Auth.Application.DTOs;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, string>? FieldErrors { get; set; }
    public List<string>? GlobalErrors { get; set; }

    public static ApiResponseDto<T> FromSuccess(T data, string message = "")
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponseDto<T> Error(string message)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message
        };
    }

    public static ApiResponseDto<T> ValidationError(Dictionary<string, string> fieldErrors)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            FieldErrors = fieldErrors
        };
    }

    public static ApiResponseDto<T> GlobalError(List<string> globalErrors)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            GlobalErrors = globalErrors
        };
    }
}
