namespace BTLWEB.ViewModels;

public class UserDetailsViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string RoleDisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public IReadOnlyList<LoginLogItemViewModel> LoginLogs { get; set; } = [];
    public IReadOnlyList<UserRoleHistoryItemViewModel> RoleHistories { get; set; } = [];
}
