using BTLWEB.Models;
using BTLWEB.ViewModels;

namespace BTLWEB.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<List<RoleOptionViewModel>> GetRoleOptionsAsync();
    Task<Role?> GetByIdAsync(int roleId);
    Task<Role?> GetByNameAsync(string roleName);
}
