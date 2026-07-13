namespace BTLWEB.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int CategoryId { get; set; }
    public int? AuthorId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }
    public string Status { get; set; } = PostStatus.Draft;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }

    public Category? Category { get; set; }
    public User? Author { get; set; }
    public User? DeletedByUser { get; set; }
}
