using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IUserPasswordRepository
{
    Task<UserPassword?> GetByUserIdAsync(Guid userId);
    Task<UserPassword> CreateAsync(UserPassword userPassword);
    Task<UserPassword> UpdateAsync(UserPassword userPassword);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> HasPasswordAsync(Guid userId);
}
