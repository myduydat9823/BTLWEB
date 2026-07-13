namespace BTLWEB.Models;

public class ArticleAdminLog
{
    public int ArticleAdminLogId { get; set; }
    public int PostId { get; set; }
    public int? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? StatusBefore { get; set; }
    public string? StatusAfter { get; set; }
    public string TitleSnapshot { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Post? Post { get; set; }
    public User? ActorUser { get; set; }
}
