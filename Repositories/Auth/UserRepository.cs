using BTLWEB.Common;
using BTLWEB.Data;
using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(int userId)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public Task<User?> GetByIdWithRoleAsync(int userId)
    {
        return _dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<PagedResult<UserListItemViewModel>> GetUsersAsync(string? search, string? role, string? status, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 5, 50);

        var query = _dbContext.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.Username.Contains(keyword)
                || x.Email.Contains(keyword)
                || x.FullName.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(x => x.Role != null && x.Role.RoleName == role);
        }

        query = status switch
        {
            "active" => query.Where(x => x.IsActive && !x.IsDeleted),
            "locked" => query.Where(x => x.LockoutEndUtc != null && x.LockoutEndUtc > DateTime.UtcNow),
            "disabled" => query.Where(x => !x.IsActive && !x.IsDeleted),
            "deleted" => query.Where(x => x.IsDeleted),
            _ => query
        };

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserListItemViewModel
            {
                UserId = x.UserId,
                Username = x.Username,
                Email = x.Email,
                FullName = x.FullName,
                RoleName = x.Role != null ? x.Role.RoleName : string.Empty,
                RoleDisplayName = x.Role != null ? x.Role.DisplayName : string.Empty,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                LockoutEndUtc = x.LockoutEndUtc,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return new PagedResult<UserListItemViewModel>
        {
            Items = users,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<UserDetailsViewModel?> GetUserDetailsAsync(int userId, Func<string?, string?> decrypt, Func<string?, DateTime?> decryptDate)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (user is null)
        {
            return null;
        }

        var logs = await _dbContext.LoginLogs
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(10)
            .Select(x => new LoginLogItemViewModel
            {
                IsSuccess = x.IsSuccess,
                FailureReason = x.FailureReason,
                IpAddress = x.IpAddress,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        var histories = await _dbContext.UserRoleHistories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ChangedAtUtc)
            .Take(10)
            .Select(x => new UserRoleHistoryItemViewModel
            {
                OldRoleName = x.OldRole != null ? x.OldRole.RoleName : null,
                NewRoleName = x.NewRole != null ? x.NewRole.RoleName : string.Empty,
                ChangedByUsername = x.ChangedByUser != null ? x.ChangedByUser.Username : null,
                Note = x.Note,
                ChangedAtUtc = x.ChangedAtUtc
            })
            .ToListAsync();

        return new UserDetailsViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            RoleName = user.Role?.RoleName ?? string.Empty,
            RoleDisplayName = user.Role?.DisplayName ?? string.Empty,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Phone = decrypt(user.PhoneEncrypted),
            Address = decrypt(user.AddressEncrypted),
            DateOfBirth = decryptDate(user.DateOfBirthEncrypted),
            IsActive = user.IsActive,
            IsDeleted = user.IsDeleted,
            LockoutEndUtc = user.LockoutEndUtc,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc,
            LoginLogs = logs,
            RoleHistories = histories
        };
    }

    public async Task UpdateUserAsync(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ChangeRoleAsync(int userId, int newRoleId, int? changedByUserId, string? note)
    {
        var user = await _dbContext.Users.FirstAsync(x => x.UserId == userId);
        var oldRoleId = user.RoleId;
        user.RoleId = newRoleId;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.UserRoleHistories.AddAsync(new UserRoleHistory
        {
            UserId = userId,
            OldRoleId = oldRoleId,
            NewRoleId = newRoleId,
            ChangedByUserId = changedByUserId,
            Note = note,
            ChangedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    public Task<bool> HasAnotherActiveAdminAsync(int userId)
    {
        return _dbContext.Users.AnyAsync(x => x.UserId != userId
            && x.IsActive
            && !x.IsDeleted
            && x.Role != null
            && x.Role.RoleName == RoleNames.Admin);
    }
}
