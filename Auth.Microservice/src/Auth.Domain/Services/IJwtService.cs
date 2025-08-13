namespace Auth.Domain.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string fullName, IEnumerable<string> roles, IDictionary<string,string>? extraClaims = null);
    bool ValidateToken(string token, out Guid? userId);
    string? GetClaimValue(string token, string claimType);
}
