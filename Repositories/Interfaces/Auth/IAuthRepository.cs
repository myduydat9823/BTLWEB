using BTLWEB.Models;

namespace BTLWEB.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<User?> GetByIdentifierAsync(string identifier);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<Role?> GetDefaultMemberRoleAsync();
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
}
