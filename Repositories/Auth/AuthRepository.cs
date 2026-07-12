using BTLWEB.Data;
using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _dbContext;

    public AuthRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdentifierAsync(string identifier)
    {
        var normalized = Normalize(identifier);

        return _dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalized || x.NormalizedUsername == normalized);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var normalized = Normalize(email);

        return _dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalized);
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        var normalized = Normalize(username);
        return _dbContext.Users.AnyAsync(x => x.NormalizedUsername == normalized);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        var normalized = Normalize(email);
        return _dbContext.Users.AnyAsync(x => x.NormalizedEmail == normalized);
    }

    public Task<Role?> GetDefaultMemberRoleAsync()
    {
        return _dbContext.Roles.FirstOrDefaultAsync(x => x.RoleName == Common.RoleNames.Member && x.IsActive);
    }

    public async Task AddUserAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
