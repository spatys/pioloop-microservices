using Auth.Domain.Entities;

namespace Auth.Domain.Services;

public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<string> roles);
    bool ValidateToken(string token, out Guid? userId);
    string? GetClaimValue(string token, string claimType);
}
