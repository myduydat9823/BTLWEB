namespace BTLWEB.Models;

public class User
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string NormalizedUsername { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? PhoneEncrypted { get; set; }
    public string? AddressEncrypted { get; set; }
    public string? DateOfBirthEncrypted { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public Role? Role { get; set; }
    public ICollection<LoginLog> LoginLogs { get; set; } = [];
    public ICollection<UserRoleHistory> RoleHistories { get; set; } = [];
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    public ICollection<Post> AuthoredPosts { get; set; } = [];
    public ICollection<Post> DeletedPosts { get; set; } = [];
    public ICollection<ArticleAdminLog> ArticleAdminLogs { get; set; } = [];
}
