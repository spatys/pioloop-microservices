namespace Auth.Application.DTOs.Request;

public class UpdateUserRoleRequest
{
    public Guid UserId { get; set; }
    public string NewRole { get; set; } = string.Empty;
}
