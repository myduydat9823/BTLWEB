namespace BTLWEB.ViewModels;

public class ArticlePreviewViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? AuthorName { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}
