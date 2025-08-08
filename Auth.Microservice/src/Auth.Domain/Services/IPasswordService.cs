namespace Auth.Domain.Services;

public interface IPasswordService
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string hash, string salt);
    string GenerateSecurePassword();
}
