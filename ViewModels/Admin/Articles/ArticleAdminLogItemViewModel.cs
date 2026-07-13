namespace BTLWEB.ViewModels;

public class ArticleAdminLogItemViewModel
{
    public string Action { get; set; } = string.Empty;
    public string? StatusBefore { get; set; }
    public string? StatusAfter { get; set; }
    public string TitleSnapshot { get; set; } = string.Empty;
    public string? ActorName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
