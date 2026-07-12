namespace BTLWEB.ViewModels;

public class UserListItemViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string RoleDisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
