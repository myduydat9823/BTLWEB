namespace BTLWEB.Models;

public class PasswordResetToken
{
    public int PasswordResetTokenId { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAtUtc { get; set; }
    public string? CreatedIpAddress { get; set; }

    public User? User { get; set; }
}
