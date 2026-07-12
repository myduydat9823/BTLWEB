namespace BTLWEB.Models;

public class LoginLog
{
    public int LoginLogId { get; set; }
    public int? UserId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
