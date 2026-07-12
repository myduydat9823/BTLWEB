using BTLWEB.Data;
using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _dbContext;

    public RoleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<RoleOptionViewModel>> GetRoleOptionsAsync()
    {
        return _dbContext.Roles
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.RoleId)
            .Select(x => new RoleOptionViewModel
            {
                RoleId = x.RoleId,
                RoleName = x.RoleName,
                DisplayName = x.DisplayName
            })
            .ToListAsync();
    }

    public Task<Role?> GetByIdAsync(int roleId)
    {
        return _dbContext.Roles.FirstOrDefaultAsync(x => x.RoleId == roleId && x.IsActive);
    }

    public Task<Role?> GetByNameAsync(string roleName)
    {
        return _dbContext.Roles.FirstOrDefaultAsync(x => x.RoleName == roleName && x.IsActive);
    }
}
