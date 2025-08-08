using Microsoft.EntityFrameworkCore;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class UserPasswordRepository : IUserPasswordRepository
{
    private readonly AuthDbContext _context;

    public UserPasswordRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<UserPassword?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserPasswords
            .FirstOrDefaultAsync(up => up.UserId == userId && up.IsActive);
    }

    public async Task<UserPassword> CreateAsync(UserPassword userPassword)
    {
        _context.UserPasswords.Add(userPassword);
        await _context.SaveChangesAsync();
        return userPassword;
    }

    public async Task<UserPassword> UpdateAsync(UserPassword userPassword)
    {
        _context.UserPasswords.Update(userPassword);
        await _context.SaveChangesAsync();
        return userPassword;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userPassword = await _context.UserPasswords.FindAsync(id);
        if (userPassword == null)
            return false;

        _context.UserPasswords.Remove(userPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasPasswordAsync(Guid userId)
    {
        return await _context.UserPasswords
            .AnyAsync(up => up.UserId == userId && up.IsActive);
    }
}
