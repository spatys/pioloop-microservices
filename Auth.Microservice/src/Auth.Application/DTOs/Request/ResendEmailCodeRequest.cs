using System.ComponentModel.DataAnnotations;

namespace Auth.Application.DTOs.Request;

public class ResendEmailCodeRequest
{
    public string Email { get; set; } = string.Empty;
}
