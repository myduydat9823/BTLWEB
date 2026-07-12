using BTLWEB.Models;
using BTLWEB.ViewModels;

namespace BTLWEB.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByIdWithRoleAsync(int userId);
    Task<PagedResult<UserListItemViewModel>> GetUsersAsync(string? search, string? role, string? status, int page, int pageSize);
    Task<UserDetailsViewModel?> GetUserDetailsAsync(int userId, Func<string?, string?> decrypt, Func<string?, DateTime?> decryptDate);
    Task UpdateUserAsync(User user);
    Task ChangeRoleAsync(int userId, int newRoleId, int? changedByUserId, string? note);
    Task<bool> HasAnotherActiveAdminAsync(int userId);
}
